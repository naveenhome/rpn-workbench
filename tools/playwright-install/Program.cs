// Installs Playwright's browsers without PowerShell.
//
// The documented route is a generated playwright.ps1 script, which needs pwsh.
// pwsh is not on a stock macOS, and asking twenty delegates to install
// PowerShell so they can install a browser is two problems where there was one.
//
// The script is only a wrapper around this entry point, so calling it directly
// does the same job with the .NET SDK they already have.
//
//   dotnet run --project tools/playwright-install              # every browser
//   dotnet run --project tools/playwright-install -- chromium  # just the one
//
// Not needed at all if the .runsettings Channel is left at "chrome" and Google
// Chrome is installed — Playwright drives that instead and downloads nothing.

var arguments = args.Length > 0
    ? new[] { "install" }.Concat(args).ToArray()
    : new[] { "install" };

Console.WriteLine($"playwright {string.Join(' ', arguments)}");

var exitCode = Microsoft.Playwright.Program.Main(arguments);

if (exitCode != 0)
{
    Console.Error.WriteLine($"Playwright install failed with exit code {exitCode}.");
    return exitCode;
}

Console.WriteLine("Done. Browsers are installed for this machine.");
return 0;
