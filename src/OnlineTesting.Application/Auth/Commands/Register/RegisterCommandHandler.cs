using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private const string EmailConflictMessage = "User with this email already exists.";

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IDbExceptionInspector _dbInspector;

    public RegisterCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        IDbExceptionInspector dbInspector)
    {
        _db = db;
        _hasher = hasher;
        _dbInspector = dbInspector;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        // TODO: user enumeration via 409 — mitigate with email confirmation flow.
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email == email, ct);
        if (exists)
            throw new ConflictException(EmailConflictMessage);

        var hash = await _hasher.HashAsync(request.Password, ct);
        var user = User.Create(email, hash, Role.Student);

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            // race с параллельной регистрацией — мапим в ту же ошибку
            throw new ConflictException(EmailConflictMessage);
        }

        return new RegisterResponse(user.Id, user.Email, user.Role);
    }
}