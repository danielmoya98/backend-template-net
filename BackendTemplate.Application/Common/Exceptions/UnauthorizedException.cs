namespace BackendTemplate.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("You are not authenticated.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
