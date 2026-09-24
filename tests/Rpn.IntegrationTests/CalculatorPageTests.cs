using System.Net;
using Xunit;

namespace Rpn.IntegrationTests;

/// <summary>
/// A smoke test, so the project is green before anybody writes anything in it.
///
/// It is deliberately thin. What belongs at this level is behaviour that spans
/// more than one class but stops short of a browser: routing, model binding,
/// filters, and whether two components that are supposed to agree actually do.
/// </summary>
public class CalculatorPageTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _app;

    public CalculatorPageTests(WebAppFixture app) => _app = app;

    [Fact]
    public async Task Home_page_is_served()
    {
        var client = _app.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task History_requires_a_signed_in_user()
    {
        // Redirects to the login page rather than serving the list. Following
        // the redirect is off so the test asserts the redirect itself.
        var client = _app.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/History");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.OriginalString ?? "");
    }
}
