namespace OnlineTesting.Domain.Authorization;

public static class Roles
{
    public const string Owner = nameof(Owner);
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string Admin = nameof(Admin);
    public const string Teacher = nameof(Teacher);
    public const string Student = nameof(Student);

    public static class Policies
    {
        public const string ContentManagement = nameof(ContentManagement);
        public const string TeacherAccess = nameof(TeacherAccess);
        public const string OwnerAccess = nameof(OwnerAccess);
    }
}