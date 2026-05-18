# Online Testing Platform — Driving Licence Theory (Uzbekistan)

## Stack
- ASP.NET Core Web API, .NET 8
- Clean Architecture: Domain / Application / Infrastructure / API
- MediatR (CQRS), FluentValidation via IPipelineBehavior
- EF Core + PostgreSQL (snake_case via EFCore.NamingConventions)
- BCrypt, JWT + refresh token rotation, HMAC for Telegram WebApp
- MinIO (S3-compatible) via AWSSDK.S3 v4 for image storage
- docker-compose: PostgreSQL on port 15432, MinIO on ports 9000/9001

## Languages
Three: `uz-latn` (default), `ru`, `uz-cyrl`. Constants in `Application/Common/Constants/Languages.cs`.

## Solution layout
```
src/
├── OnlineTesting.Domain/          (entities, base Entity, INotification domain events)
├── OnlineTesting.Application/     (CQRS handlers, validators, behaviors, interfaces)
├── OnlineTesting.Infrastructure/  (EF Core, ApplicationDbContext, JWT, Storage)
└── OnlineTesting.API/             (Controllers, Middleware, Localization)
```

## Conventions

**Domain**
- Private constructors, static factories, no setters — only behaviour methods
- Base `Entity` (non-generic): `Guid Id { get; protected set; }`, two ctors
- Domain events via `MediatR.INotification`
- Aggregate roots create entities through object-initializer with `Id = Guid.NewGuid()`

**Application**
- Folder pattern: `Tests/{Admin|Solutions}/{EntityType}/Commands|Queries/{Feature}/{Feature}Command.cs + Handler.cs + Validator.cs`
- Infrastructure interfaces in `Common/Interfaces/`: `IApplicationDbContext`, `IJwtService`, `IPasswordHasher`, `IRequestContext`, `ICurrentUser`, `IDbExceptionInspector`, `ITelegramAuthValidator`, `ILanguageContext`, `ISubscriptionChecker`, `IStorageService`
- `ILanguageContext` exposes `RequestedLanguage` and `DefaultLanguage`
- Custom exceptions: `ValidationException`, `ConflictException`, `NotFoundException`, `UnauthorizedException`
- `ValidationBehavior` via `IPipelineBehavior`
- `PagedResult<T>` in `Common/Models/`

**Infrastructure**
- All entity configs via `IEntityTypeConfiguration<T>` in `Persistence/Configurations/`
- `OnModelCreating` uses `ApplyConfigurationsFromAssembly` — new configs picked up automatically
- `PostgresExceptionInspector.IsUniqueConstraintViolation(Exception)` for SQLSTATE 23505
- `SubscriptionCheckerStub` always returns `true` (placeholder)
- `MinioStorageService`: `UseChunkEncoding = false` (required for HTTP; DisablePayloadSigning needs HTTPS)
- `BucketInitializer`: IHostedService — creates bucket + sets public-read policy on startup

**API**
- `ExceptionHandlingMiddleware` maps custom exceptions to RFC 7807 `ProblemDetails`
- JWT: `MapInboundClaims = false`, `NameClaimType = "sub"`, `RoleClaimType = ClaimTypes.Role`
- Authorization policy `ContentManagement` = Owner + SuperAdmin + Admin
- `JwtBearerEvents.OnAuthenticationFailed → NoResult()` (anonymous fallback for `[AllowAnonymous]`)
- `LanguageMiddleware` after Auth; reads `?lang=` (priority) or `Accept-Language`
- Swagger with Bearer security scheme

**Security**
- Constant-time defence on login (`CryptographicOperations.FixedTimeEquals`)
- Refresh token rotation with replay detection
- Race-condition defence: UNIQUE indexes + catch `DbUpdateException` via `IDbExceptionInspector`
- Reserved domain `@telegram.local` blocked on registration

## Roles
Registration grants `Role = Student (5)`. To get Admin — UPDATE the user row directly in DB. Roles stored as strings in JWT (`"Student"`, `"Admin"`), not numbers.

## Module status

### ✅ Auth (done, tested)
Endpoints: `/auth/register`, `/auth/login`, `/auth/refresh`, `/auth/telegram`, `/auth/logout`
Entities: `User`, `RefreshToken`, `ExternalLogin`

### ✅ B.1 — Topic / Question / Answer CRUD + multi-language (done, 8 scenarios passed in Swagger)
- Domain: `Language` (PK = string code), `Topic` + `TopicTranslation`, `Question` + `QuestionTranslation` + `Answer` + `AnswerTranslation`
- Translation tables (Variant A); default uz-latn required; fallback chain: requested → default → "(no translation)"
- DTOs carry `language` and `isFallback` flags
- `QuestionTranslation` has nullable `Explanation` field
- 18 admin endpoints + 2 public; guests see only demo content
- Tables: `languages` (seeded), `topics`, `topic_translations`, `questions`, `question_translations`, `answers`, `answer_translations`
- Partial unique indexes: `ux_languages_default`, `ux_topics_demo`, `ux_answers_question_correct`
- Cascade: translations cascade with parent; answers cascade with question; questions → topics RESTRICT

### ✅ B.2 — Bilet CRUD (done, 13 scenarios passed in Swagger)
- A Bilet is exactly 20 references to existing Questions; numbered 1..N (62 expected)
- One question lives in exactly one bilet (enforced by unique index on `bilet_questions.question_id`)
- One demo bilet at a time (partial unique on `bilets.is_demo`)
- No translations, no `TopicId`, just `Number`
- Tables: `bilets`, `bilet_questions` (composite PK, check `order_index BETWEEN 1 AND 20`)
- Cascade: bilet → bilet_questions; bilet_questions → questions RESTRICT
- `UpdateBilet` uses transactional replace (DELETE then INSERT in two SaveChanges within `BeginTransactionAsync`) — required because `ux_bilet_questions_question` unique forbids holding old+new for same question_id
- `DeleteQuestion` updated to check usage in bilets → ConflictException if used
- Admin endpoints: CRUD + activate/deactivate + mark-demo/unmark-demo + list/get
- Public: `GET /bilets` (guest sees demo only), `GET /bilets/{id}` (guest 401 unless demo)
- `IsCorrect` and `Explanation` returned in GET (textbook mode)
- `IApplicationDbContext` gained `BeginTransactionAsync` method

### ✅ B.3 — Attempt + 5 flows (done, 10 scenarios passed in Swagger)
- Attempt is a single-session test; no resume/save-progress
- 5 flow types: Bilet(1), Topic(2), Custom(3), Exam(4), Marathon(5)
- Bilet: 20 fixed questions from a bilet in order
- Topic: ALL active questions from one topic, random order
- Custom: random N questions from selected topics (topicIds=null → all topics)
- Exam: random 20 from all active questions; ExamTimeLimitSeconds=1500 (25 min, enforced client-side)
- Marathon: all active questions, random order
- Exam rules: 3rd mistake → auto-fail immediately (isFinished:true in SubmitAnswer response); on Finish → Passed if correctCount≥18 else Failed
- AttemptStatus: InProgress(0), Completed(1), Passed(2), Failed(3)
- Tables: `attempts`, `attempt_questions` (composite PK attempt_id+question_id)
- Cascade: attempt → attempt_questions; attempt_questions → questions RESTRICT
- Student endpoints: POST /attempts, GET /attempts/{id}, POST /attempts/{id}/answer, POST /attempts/{id}/finish
- GET /attempts/{id} returns remainingSeconds for Exam+InProgress
- Textbook mode: isCorrect shown immediately after each answer

### ✅ B.4 — Image upload (done, 7 scenarios passed in Swagger)
- `Question.ImageKey` stores the MinIO object key (e.g. `questions/{id}.png`)
- `IStorageService`: `UploadAsync`, `DeleteAsync`, `GetPublicUrl`
- `MinioStorageService` uses `UseChunkEncoding = false` (HTTP-compatible, not DisablePayloadSigning)
- `BucketInitializer` IHostedService creates bucket + public-read policy on startup
- `UploadQuestionImage`: validates content type (image/*), max 5 MB, deletes old image if exists, key = `questions/{id}.{ext}`
- `DeleteQuestionImage`: deletes from storage + clears ImageKey; 404 if no image
- `DeleteQuestion` auto-deletes image from storage if present
- `GET /admin/questions/{id}` and list both return `imageUrl` (full MinIO URL) alongside `imageKey`
- Admin endpoints: `POST /admin/questions/{id}/image` (multipart/form-data), `DELETE /admin/questions/{id}/image`
- Storage config in `appsettings.json` → `Storage:{ Endpoint, AccessKey, SecretKey, BucketName, UseHttps }`

### ✅ B.5 — My Progress / analytics (done, 8 scenarios passed in Swagger)
- New table: `user_daily_activities` (user_id, activity_date) — composite PK, for streak tracking
- Streak tracked on every `SubmitAnswer` — upserts today's record for the user
- 4 student endpoints: `GET /progress/dashboard`, `/progress/topics`, `/progress/errors`, `/progress/history`
- Dashboard: currentStreak, longestStreak, level (6 levels by totalCorrect), accuracyPercent, examPassPrediction (0–95), weakTopics (accuracy<65%, min 5 answered), recentAttempts
- Topics: all topics ordered by OrderIndex, with totalAnswered/correctCount/accuracyPercent/grade
- Grade thresholds: Отлично ≥85%, Хорошо ≥65%, Нужно повторить ≥40%, Критично <40%, Не изучено <5 answered
- Errors: top 20 questions by error count, with errorRatePercent and topic info
- History: paginated attempt list, filterable by flowType
- Exam prediction formula: 60% from last 5 exam scores + 25% topic coverage (≥65% accuracy) + 15% practice volume (cap 500 questions)
- Helpers GetGrade/GetTopicName/ComputePrediction reused across handlers via GetDashboardHandler static methods

### ✅ B.6.1 — Teacher Core (done, 11 scenarios passed in Swagger, review fixes applied)
- Teacher is NOT a separate account type — it's a role elevation on top of Student
- New Role: Teacher=4 (Owner=1, SuperAdmin=2, Admin=3, Teacher=4, Student=5)
- New policy: `TeacherAccess` = Owner + SuperAdmin + Admin + Teacher
- User.SetRole(role) added to User domain entity
- **Teacher Application System**: Student submits application → Admin approves/rejects → role changes to Teacher
  - Partial unique index: only one Pending application per user at a time
  - Approve: sets application.Status=Approved + user.SetRole(Teacher)
  - Reject: sets application.Status=Rejected + stores RejectionReason
  - Student can see their latest application via GET /teacher-applications/my
- **Group System**: Teacher creates groups with auto-generated 8-char invite code (cryptographically random)
  - Students join by invite code via POST /groups/join
  - Teacher can view members, remove members, delete group (cascade deletes members)
  - Unique index on invite_code
- Student endpoints: POST /teacher-applications, GET /teacher-applications/my, POST /groups/join
- Admin endpoints: GET /admin/teacher-applications (filterable by status), POST /{id}/approve, POST /{id}/reject
- Teacher endpoints: POST/GET /teacher/groups, DELETE /teacher/groups/{id}, GET /teacher/groups/{id}/members, DELETE /teacher/groups/{id}/members/{userId}
- Tables: `teacher_applications`, `groups`, `group_members`
- Fix: GetGroupMembers used OrderBy after Join with record constructor — moved OrderBy before Join

### ✅ B.6.2 — Teacher Test Links (done, 10 scenarios passed in Swagger)
- `TestLink` entity: TeacherId, Title, Code (8-char random), FlowType, BiletId, TopicIds (uuid[]), QuestionCount, GroupId (label only), MaxAttempts (default 1), ExpiresAt (default now+1d), IsActive
- `Attempt.TestLinkId (Guid?)` added — test link attempts excluded from all student progress (dashboard, topics, errors, history, streak)
- Teacher endpoints: `POST/GET /teacher/test-links`, `PATCH /{id}/deactivate`, `GET /{id}/results`
- Public endpoints: `GET /test-links/{code}` (info + attemptsUsed), `POST /test-links/{code}/start` → attemptId
- Access: subscription check + MaxAttempts + ExpiresAt; GroupId is metadata only (no access restriction)
- Tables: `test_links` (uuid[] for TopicIds), `attempts.test_link_id` nullable FK

## Backlog (not started)
- **B.7** — Subscription module + payments

## Working agreement
1. Architecture/discussion before code; no surprise refactors
2. After code, self-review with 🔴/🟡/🟢 priorities
3. After review, fixes by user approval
4. Migrations: show Up() for approval before `database update`
5. After applying, run Swagger scenarios
6. Build green is the gate between phases — never proceed without `dotnet build` clean

## Common commands

Build:
```
dotnet build
```

Add migration:
```
dotnet ef migrations add <Name> --project src/OnlineTesting.Infrastructure --startup-project src/OnlineTesting.API
```

Apply migration:
```
dotnet ef database update --project src/OnlineTesting.Infrastructure --startup-project src/OnlineTesting.API
```

Inspect DB:
```
docker exec -it onlinetesting-pg psql -U postgres -d online_testing -c "\d <table>"
```

## Style preferences (from previous sessions)
- Russian language for chat
- Compact explanations, no over-formatting
- Always state assumptions inline; ask only when truly blocked
