using Microsoft.Playwright.MSTest;

namespace Rpn.UiTests;

/// <summary>
/// Base class for tests that drive the application in a real browser.
///
/// These tests do not start the application. Run it first:
///
///     dotnet run --project src/Rpn.Web
///
/// and leave it running. Point the tests somewhere else by setting RPN_BASE_URL.
///
/// A UI test is the slowest and most brittle thing in the pyramid, so there
/// should be few of them, and each one should earn its place by covering
/// something no cheaper test can reach.
/// </summary>
public abstract class AppPageTest : PageTest
{
    protected static string BaseUrl =>
        Environment.GetEnvironmentVariable("RPN_BASE_URL") ?? "http://localhost:5080";

    protected static string Url(string path) => $"{BaseUrl}{path}";

    /// <summary>
    /// Signs in through the real login form, as a real person would.
    /// The two accounts are seeded in development: alice@example.com and
    /// bob@example.com, both with the password below.
    /// </summary>
    protected async Task SignInAsync(string email, string password = "Workshop123!")
    {
        await Page.GotoAsync(Url("/Identity/Account/Login"));
        await Page.FillAsync("input[name='Input.Email']", email);
        await Page.FillAsync("input[name='Input.Password']", password);
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForURLAsync(u => !u.Contains("/Identity/Account/Login"));
    }

    protected async Task SignOutAsync()
    {
        await Page.GotoAsync(Url("/"));
        var logout = Page.Locator("button:has-text('Logout'), a:has-text('Logout')").First;
        if (await logout.CountAsync() > 0)
        {
            await logout.ClickAsync();
        }
        await Page.Context.ClearCookiesAsync();
    }
}
