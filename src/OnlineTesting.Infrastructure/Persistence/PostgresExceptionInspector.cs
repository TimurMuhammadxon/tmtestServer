using Microsoft.EntityFrameworkCore;
using Npgsql;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Persistence;

public class PostgresExceptionInspector : IDbExceptionInspector
{
    private const string UniqueViolationSqlState = "23505";

    public bool IsUniqueConstraintViolation(Exception exception)
        => exception is DbUpdateException due
           && due.InnerException is PostgresException pg
           && pg.SqlState == UniqueViolationSqlState;
}