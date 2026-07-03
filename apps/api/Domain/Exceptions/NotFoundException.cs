namespace BrowserAgent.Api.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string resource, object key)
        : base("NOT_FOUND", $"Resource '{resource}' with key '{key}' was not found.")
    {
    }
}
