using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rpn.UiTests;

/// <summary>
/// FACILITATOR ONLY — this is the answer to the module 7 lab.
///
/// D9: HistoryController.Details fetches a calculation by id and never checks
/// who owns it. Any signed-in user can read any other user's history by asking
/// for the id directly.
///
/// EXPECT THIS TEST TO FAIL on an unfixed repository. That is the point. Every
/// unit test in the solution stays green while this one goes red, which is the
/// whole argument for the pyramid in one run.
///
/// It fails at the last assertion: Bob can see Alice's expression.
/// </summary>
[TestClass]
public class HistoryPrivacyTests : AppPageTest
{
    [TestMethod]
    public async Task One_user_cannot_read_another_users_calculation()
    {
        // ── Alice evaluates something only she should be able to see
        await SignInAsync("alice@example.com");
        await Page.GotoAsync(Url("/Calculator"));
        await Page.FillAsync("input[name='Expression']", "11 7 *");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page.Locator("p.result")).ToContainTextAsync("77");

        // The history list links each row to its own id.
        await Page.GotoAsync(Url("/History"));
        var href = await Page.Locator("a:has-text('Open')").First.GetAttributeAsync("href");
        Assert.IsNotNull(href, "Alice's history has no rows — did the calculation save?");

        var id = Regex.Match(href, @"(\d+)$").Groups[1].Value;
        Assert.AreNotEqual(string.Empty, id, $"could not read an id out of '{href}'");

        // ── Bob asks for it directly
        await SignOutAsync();
        await SignInAsync("bob@example.com");
        await Page.GotoAsync(Url($"/History/Details/{id}"));

        // Bob is signed in, so this is not about authentication. It is about
        // whether the application checks that the row belongs to him.
        var body = await Page.Locator("body").InnerTextAsync();
        Assert.IsFalse(body.Contains("11 7 *"),
            "Bob can read Alice's calculation. HistoryController.Details fetches "
            + "by id and never compares the owner to the signed-in user.");
    }
}
