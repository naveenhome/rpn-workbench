using Microsoft.Extensions.DependencyInjection;
using Rpn.Web.Services;
using Xunit;

namespace Rpn.IntegrationTests;

/// <summary>
/// FACILITATOR ONLY — this is the answer to part of the module 8 lab.
///
/// D5: there are two Normalise methods. CalculatorController has a private
/// static one; HistoryService has a public one. They are identical except that
/// the second strips a trailing '='.
///
/// This is a characterisation test, not a bug report. It does not claim the
/// difference is wrong — it pins the difference in place, so that a pair who
/// extracts the duplication finds out immediately if they lost it. That is the
/// distinction worth drawing in the room: a test can assert what ought to be
/// true, or it can record what is currently true so a refactor cannot change it
/// by accident. Most refactoring safety nets are the second kind.
/// </summary>
public class NormalisationCharacterisationTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _app;

    public NormalisationCharacterisationTests(WebAppFixture app) => _app = app;

    private HistoryService Service()
    {
        var scope = _app.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<HistoryService>();
    }

    [Theory]
    [InlineData("3 4 +", "3 4 +")]
    [InlineData("  3   4   +  ", "3 4 +")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void History_normalisation_collapses_whitespace(string input, string expected)
    {
        Assert.Equal(expected, Service().Normalise(input));
    }

    [Fact]
    public void History_normalisation_strips_one_trailing_equals()
    {
        // The controller's copy does not do this. If you extract the two into
        // one method and this test still passes, check the calculator still
        // rejects "3 4 + =" — because that rejection is the other half of the
        // behaviour, and no test covers it.
        Assert.Equal("3 4 +", Service().Normalise("3 4 + ="));
    }
}
