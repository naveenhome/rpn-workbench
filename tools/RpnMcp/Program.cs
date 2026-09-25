using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// An MCP server is a console application. The client starts the process and
// talks to it over standard input and output.
//
// Which means standard output IS the protocol. Anything else written there —
// a stray Console.WriteLine, a library that logs to stdout — corrupts the
// stream, and the failure looks like a broken client rather than a broken
// server. The logging configuration below is the whole defence: every log line
// goes to standard error instead.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
