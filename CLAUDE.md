Хорошо, переезд в Claude Code — разумный шаг для проекта такого масштаба. Там я смогу сам читать файлы, делать правки, запускать `dotnet build` и `dotnet ef` команды — без копипасты туда-сюда.

Вот что нужно сделать, чтобы новый я в Claude Code сразу включился в работу без потери контекста.

## 1. Создай файл-резюме в корне проекта

Положи в корень репозитория файл `CLAUDE.md` — Claude Code автоматически читает его при старте сессии. Это аналог того длинного промпта, который мы делали при переезде из B.1 в B.2, но более компактный и структурированный под IDE-формат работы.

Я подготовил содержимое — скопируй целиком:

```markdown
# Online Testing Platform — Driving Licence Theory (Uzbekistan)

## Stack
- ASP.NET Core Web API, .NET 8
- Clean Architecture: Domain / Application / Infrastructure / API
- MediatR (CQRS), FluentValidation via IPipelineBehavior
- EF Core + PostgreSQL (snake_case via EFCore.NamingConventions)
- BCrypt, JWT + refresh token rotation, HMAC for Telegram WebApp
- PostgreSQL in Docker: container `onlinetesting-pg`, port 15432, db `online_testing`

## Languages
Three: `uz-latn` (default), `ru`, `uz-cyrl`. Constants in `Application/Common/Constants/Languages.cs`.

## Solution layout
```
src/
├── OnlineTesting.Domain/          (entities, base Entity, INotification domain events)
├── OnlineTesting.Application/     (CQRS handlers, validators, behaviors, interfaces)
├── OnlineTesting.Infrastructure/  (EF Core, ApplicationDbContext, JWT, repositories)
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
- Infrastructure interfaces in `Common/Interfaces/`: `IApplicationDbContext`, `IJwtService`, `IPasswordHasher`, `IRequestContext`, `ICurrentUser`, `IDbExceptionInspector`, `ITelegramAuthValidator`, `ILanguageContext`, `ISubscriptionChecker`
- `ILanguageContext` exposes `RequestedLanguage` and `DefaultLanguage`
- Custom exceptions: `ValidationException`, `ConflictException`, `NotFoundException`, `UnauthorizedException`
- `ValidationBehavior` via `IPipelineBehavior`
- `PagedResult<T>` in `Common/Models/`

**Infrastructure**
- All entity configs via `IEntityTypeConfiguration<T>` in `Persistence/Configurations/`
- `OnModelCreating` uses `ApplyConfigurationsFromAssembly` — new configs picked up automatically
- `PostgresExceptionInspector.IsUniqueConstraintViolation(Exception)` for SQLSTATE 23505
- `SubscriptionCheckerStub` always returns `true` (placeholder)

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

## Backlog (not started)
- **B.4** — Image upload (storage for `Question.ImageKey`)
- **B.5** — My Progress / analytics
- **B.6** — Teacher flow
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
- Show changes file-by-file with full content (not patches), batched 22 files max per response
- Always state assumptions inline; ask only when truly blocked
```

## 2. Закоммить файл

```bash
git add CLAUDE.md
git commit -m "docs: add CLAUDE.md context for Claude Code"
```

## 3. Что новый я будет делать в Claude Code

Когда откроешь проект в Antigravity и запустишь Claude Code — он автоматически прочитает `CLAUDE.md` и поймёт весь контекст. Тебе нужно будет просто сказать в первом сообщении что-то вроде:

> «Продолжаем работу над B.2. Код написан, миграция применена. Осталось пройти 13 сценариев тестирования через Swagger. Помоги по ходу если что-то не так пойдёт.»

Или если хочешь сразу к B.3:

> «B.2 готов и протестирован. Переходим к B.3 — Attempt и flow тестирования. Начни с архитектуры.»

## 4. Бонус — что Claude Code умеет лучше чем чат

- Сам читает любой файл в проекте — больше не надо копировать содержимое
- Сам делает правки через инструменты редактирования
- Сам запускает `dotnet build`, `dotnet ef`, `git` команды
- Видит ошибки компиляции и исправляет в цикле
- Делает коммиты с осмысленными сообщениями

То есть процесс «партия 22 файла → ты раскладываешь → говоришь успешно → следующая партия» исчезнет. Будет одна итерация: я делаю изменения → запускаю билд → если красный, исправляю → если зелёный, говорю «готово».

## 5. Один совет на старте

Когда впервые запустишь Claude Code в этом проекте — попроси его **сначала прочитать `CLAUDE.md` и подтвердить, что он понял контекст**, прежде чем что-то делать. Так убедишься, что переезд прошёл без потерь.

Удачи! Если завтра в Claude Code что-то будет работать не так как мы тут договаривались — просто скажи «вернёмся к чату» и я снова окажусь здесь.