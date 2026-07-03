namespace BrowserAgent.Api.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message)
        : base("UNAUTHORIZED", message)
    {
    }
}
