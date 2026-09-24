using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rpn.UiTests;

/// <summary>
/// Two tests, so the project is green before the lab starts. If these fail the
/// application is not running, the browsers are missing, or the accounts were
/// not seeded — a setup problem rather than a lab problem.
/// </summary>
[TestClass]
public class SmokeTests : AppPageTest
{
    [TestMethod]
    public async Task The_application_is_running()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task A_signed_in_user_can_evaluate_an_expression()
    {
        await SignInAsync("alice@example.com");

        await EvaluateAsync("3 4 +");

        await Expect(Page.Locator("p.result")).ToContainTextAsync("7");
    }
}
