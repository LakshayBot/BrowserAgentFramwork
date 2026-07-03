namespace BrowserAgent.Api.Browser.Exceptions;

public class BrowserException : Exception
{
    public string Code { get; }

    public BrowserException(string code, string message) : base(message)
    {
        Code = code;
    }

    public BrowserException(string code, string message, Exception inner) : base(message, inner)
    {
        Code = code;
    }
}
