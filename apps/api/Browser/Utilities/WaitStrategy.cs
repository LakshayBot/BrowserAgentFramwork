using Microsoft.Playwright;

namespace BrowserAgent.Api.Browser.Utilities;

public static class WaitStrategy
{
    public static float DefaultTimeoutSeconds { get; set; } = 30;

    public static LocatorWaitForOptions Visible()
    {
        return new LocatorWaitForOptions
        {
            State = Microsoft.Playwright.WaitForSelectorState.Visible,
            Timeout = DefaultTimeoutSeconds * 1000
        };
    }

    public static LocatorWaitForOptions Attached()
    {
        return new LocatorWaitForOptions
        {
            State = Microsoft.Playwright.WaitForSelectorState.Attached,
            Timeout = DefaultTimeoutSeconds * 1000
        };
    }
}
