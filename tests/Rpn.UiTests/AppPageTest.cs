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
/// One rule holds this file together: never click a bare button[type='submit'].
/// Every page here carries the layout's sign-out form, which contains a submit
/// button of its own and appears first in the document. A selector that matches
/// two things does not fail — it picks one, and the test then reports something
/// unrelated several lines later. Scope every click to the form that owns it.
/// </summary>
public abstract class AppPageTest : PageTest
{
    // No TestContext property is declared here. PageTest already inherits one
    // from WorkerAwareTest, and re-declaring it means MSTest injects into this
    // copy while Playwright's own teardown reads the base one, which is then
    // null — a NullReferenceException in BrowserTearDown on every test.

    protected static string BaseUrl =>
        Environment.GetEnvironmentVariable("RPN_BASE_URL") ?? "http://localhost:5080";

    protected static string Url(string path) => $"{BaseUrl}{path}";

    /// <summary>The calculator's own form, not the layout's sign-out form.</summary>
    protected ILocator CalculatorForm => Page.Locator("form:has(#Expression)");

    /// <summary>
    /// Signs in through the real login form. Both accounts are seeded in
    /// development: alice@example.com and bob@example.com.
    /// </summary>
    protected async Task SignInAsync(string email, string password = "Workshop123!")
    {
        await Page.GotoAsync(Url("/Identity/Account/Login"));

        var form = Page.Locator("form:has(input[name='Input.Email'])");
        await form.Locator("input[name='Input.Email']").FillAsync(email);
        await form.Locator("input[name='Input.Password']").FillAsync(password);
        await form.Locator("button[type='submit']").ClickAsync();

        await Page.WaitForURLAsync(u => !u.Contains("/Identity/Account/Login"),
            new() { Timeout = 10_000 });

        if (await Page.Locator("form[action*='Logout']").CountAsync() == 0)
        {
            await DumpAsync("signin-failed");
            Assert.Fail($"Signed in as {email} but the page has no sign-out form. "
                        + $"Landed on {Page.Url}. Is the account seeded? Seeding only "
                        + "runs when ASPNETCORE_ENVIRONMENT is Development.");
        }
    }

    protected async Task SignOutAsync()
    {
        await Page.GotoAsync(Url("/"));
        var logout = Page.Locator("form[action*='Logout'] button[type='submit']");
        if (await logout.CountAsync() > 0) await logout.ClickAsync();
        await Page.Context.ClearCookiesAsync();
    }

    /// <summary>Evaluates an expression through the calculator page.</summary>
    protected async Task EvaluateAsync(string expression)
    {
        await Page.GotoAsync(Url("/Calculator"));
        await CalculatorForm.Locator("#Expression").FillAsync(expression);
        await CalculatorForm.Locator("button[type='submit']").ClickAsync();
    }

    /// <summary>
    /// Writes a screenshot and the page HTML beside the test binaries, so a
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
