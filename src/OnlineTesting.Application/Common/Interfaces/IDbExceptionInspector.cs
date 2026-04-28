namespace OnlineTesting.Application.Common.Interfaces;

public interface IDbExceptionInspector
{
    bool IsUniqueConstraintViolation(Exception exception);
}