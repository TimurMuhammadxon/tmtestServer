namespace OnlineTesting.Application.Common.Interfaces;

public interface IRequestContext
{
    string? IpAddress { get; }
    string? UserAgent { get; }
}