namespace OnlineTesting.Application.Common.Exceptions;

public class PaymeRpcException : Exception
{
    public int Code { get; }
    public PaymeRpcException(int code, string message) : base(message) => Code = code;
}
