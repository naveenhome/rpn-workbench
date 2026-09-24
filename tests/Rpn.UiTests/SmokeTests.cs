using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rpn.UiTests;

/// <summary>
/// One test, so the project is green before the lab starts. If this fails, the
/// application is not running or the browsers are not installed — either way
/// it is a setup problem, not a lab problem.
/// </summary>
[TestClass]
public class SmokeTests : AppPageTest
{
    [TestMethod]
    public async Task The_application_is_running()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".+"));
    }

    [TestMethod]
    public async Task A_signed_in_user_can_evaluate_an_expression()
    {
        await SignInAsync("alice@example.com");

        await Page.GotoAsync(Url("/Calculator"));
        await Page.FillAsync("input[name='Expression']", "3 4 +");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page.Locator("p.result")).ToContainTextAsync("7");
    }
}
