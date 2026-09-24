#!/usr/bin/env python3
"""
A poor substitute for a compiler, written because there was not one available.

Maps the types this solution uses to the namespace that declares them, then
reports any file that names a type without importing it. It catches exactly one
class of mistake — CS0246 — which is the one that has cost the most round trips
here. Delete it once the build is reliably green.

    python3 tools/check-usings.py
"""
import pathlib, re, sys

NS = {
    'Microsoft.Playwright': ['ILocator', 'IPage', 'IBrowser', 'IBrowserContext',
                             'IBrowserType', 'PlaywrightException', 'IResponse',
                             'LocatorAssertions', 'IElementHandle'],
    'Microsoft.Playwright.MSTest': ['PageTest', 'ContextTest', 'BrowserTest'],
    'Microsoft.VisualStudio.TestTools.UnitTesting':
        ['TestClass', 'TestMethod', 'TestCleanup', 'TestInitialize',
         'TestContext', 'UnitTestOutcome', 'ClassInitialize', 'AssemblyInitialize'],
    'Xunit': ['Fact', 'Theory', 'InlineData', 'IClassFixture', 'ITestOutputHelper'],
    'Microsoft.AspNetCore.Mvc.Testing':
        ['WebApplicationFactory', 'WebApplicationFactoryClientOptions'],
    'Microsoft.AspNetCore.Hosting': ['IWebHostBuilder'],
    'Microsoft.Extensions.DependencyInjection': ['IServiceCollection', 'ServiceDescriptor'],
    'Microsoft.EntityFrameworkCore': ['DbContextOptions', 'DbContext'],
    'Microsoft.Data.Sqlite': ['SqliteConnection'],
    'System.Data.Common': ['DbConnection'],
    'System.Net': ['HttpStatusCode'],
    'System.Text.RegularExpressions': ['Regex'],
}
# Assert exists in both Xunit and MSTest, so it cannot identify a namespace.
AMBIGUOUS = {'Assert'}

problems = 0
for cs in sorted(pathlib.Path('.').rglob('*.cs')):
    if {'obj', 'bin', '.git'} & set(cs.parts):
        continue
    src = cs.read_text()
    body = re.sub(r'^using [^\n]+;\n', '', src, flags=re.M)   # ignore the imports themselves
    body = re.sub(r'//[^\n]*', '', body)                      # and line comments
    body = re.sub(r'/\*.*?\*/', '', body, flags=re.S)         # and block comments
    imports = set(re.findall(r'^using ([\w.]+);', src, re.M))

    for ns, types in NS.items():
        if ns in imports:
            continue
        for t in types:
            if t in AMBIGUOUS:
                continue
            # a bare type name, not preceded by a dot (which would mean it is
            # already qualified, e.g. Microsoft.Playwright.Program)
            if re.search(rf'(?<![\w.]){re.escape(t)}\b', body):
                print(f'  {cs}: uses {t} but does not import {ns}')
                problems += 1

print(f'\n{problems} missing import(s)')
sys.exit(1 if problems else 0)
