using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rpn.UiTests;

/// <summary>
/// Base class for tests that drive the application in a real browser.
///
/// These tests do not start the application. Run it first:
///
///     dotnet run --project src/Rpn.Web
///
/// and leave it running. Point the tests elsewhere with RPN_BASE_URL.
///
/// A UI test is the slowest and most brittle thing in the pyramid, so there
/// should be few of them, and each should cover something no cheaper test can
/// reach. The same brittleness is why a failing one writes down what it saw:
/// "element not found" tells you nothing on its own.
/// </summary>
public abstract class AppPageTest : PageTest
{
    public TestContext TestContext { get; set; } = null!;

    protected static string BaseUrl =>
        Environment.GetEnvironmentVariable("RPN_BASE_URL") ?? "http://localhost:5080";

    protected static string Url(string path) => $"{BaseUrl}{path}";

    /// <summary>
    /// Signs in through the real login form, as a person would. The two
    /// accounts are seeded in development: alice@example.com and
    /// bob@example.com, both with the password below.
    /// </summary>
    protected async Task SignInAsync(string email, string password = "Workshop123!")
    {
        await Page.GotoAsync(Url("/Identity/Account/Login"));
        await Page.FillAsync("input[name='Input.Email']", email);
        await Page.FillAsync("input[name='Input.Password']", password);
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForURLAsync(u => !u.Contains("/Identity/Account/Login"),
            new() { Timeout = 10_000 });

        // Fail here, with a reason, rather than three lines later with
        // "element not found" on a page that is actually the login form.
        var signedIn = await Page.Locator("form[action*='Logout'], a[href*='Logout']").CountAsync();
        if (signedIn == 0)
        {
            await DumpAsync("signin-failed");
            Assert.Fail($"Signed in as {email} but found no sign-out control. "
                        + $"Landed on {Page.Url}. Is the account seeded? "
                        + "Seeding only runs when ASPNETCORE_ENVIRONMENT is Development.");
        }
    }

    protected async Task SignOutAsync()
    {
        await Page.GotoAsync(Url("/"));
        var logout = Page.Locator("button:has-text('Logout'), a:has-text('Logout')").First;
        if (await logout.CountAsync() > 0) await logout.ClickAsync();
        await Page.Context.ClearCookiesAsync();
    }

    /// <summary>
    /// Writes a screenshot and the page's HTML beside the test binaries, so a
    /// failure can be read after the browser has gone.
    /// </summary>
    protected async Task DumpAsync(string label)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "failures");
        Directory.CreateDirectory(dir);
        var stem = Path.Combine(dir, $"{label}-{DateTime.Now:HHmmss}");

        await Page.ScreenshotAsync(new() { Path = stem + ".png", FullPage = true });
        await File.WriteAllTextAsync(stem + ".html", await Page.ContentAsync());

        TestContext.WriteLine($"url  : {Page.Url}");
        TestContext.WriteLine($"shot : {stem}.png");
        TestContext.WriteLine($"html : {stem}.html");

        // The two elements that explain most failures on this application.
        foreach (var sel in new[] { "p.error", "p.result", "div.validation-summary-errors", "h1" })
        {
            var loc = Page.Locator(sel);
            if (await loc.CountAsync() > 0)
                TestContext.WriteLine($"{sel,-32} {(await loc.First.InnerTextAsync()).Trim()}");
        }
    }

    [TestCleanup]
    public async Task CaptureOnFailure()
    {
        if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
        {
            try { await DumpAsync(TestContext.TestName ?? "test"); }
            catch (Exception e) { TestContext.WriteLine($"could not capture: {e.Message}"); }
        }
    }
}
