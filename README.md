# Serilog.Filters.Expressions [![Build status](https://ci.appveyor.com/api/projects/status/wnh0ig2udlld9oe4?svg=true)](https://ci.appveyor.com/project/serilog/serilog-filters-expressions) [![NuGet Release](https://img.shields.io/nuget/v/Serilog.Filters.Expressions.svg)](https://nuget.org/packages/serilog.filters.expressions)

Expression-based event filtering for [Serilog](https://serilog.net).

```csharp
var expr = "@Level = 'Information' and AppId is not null and Items[?] like 'C%'";

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("AppId", 10)
    .Filter.ByIncludingOnly(expr)
    .WriteTo.Console()
    .CreateLogger();

// Printed
Log.Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
Log.Information("Cart contains {@Items}", new[] { "Peanuts", "Chocolate" });

// Not printed
Log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
Log.Information("Cart contains {@Items}", new[] { "Apricots" });

Log.CloseAndFlush();
```

### Getting started

Install _Serilog.Filters.Expressions_ from NuGet:

```powershell
Install-Package Serilog.Filters.Expressions
```

Add `Filter.ByIncludingOnly(fiterExpression)` or `Filter.ByExcluding(fiterExpression)` calls to `LoggerConfiguration`.

### Syntax

The syntax is based on SQL, with added support for object structures, arrays, and regular expressions.

| Category | Examples |
| :--- | :--- |
| **Literals** | `123`, `123.4`, `'Hello'`, `true`, `false`, `null` |
| **Properties** | `A`, `A.B`, `@Level`, `@Timestamp`, `@Exception`, `@Message`, `@MessageTemplate`, `@Properties['A-b-c']` |
| **Comparisons** | `A = B`, `A <> B`, `A > B`, `A >= B`, `A is null`, `A is not null`, `A in [1, 2]` |
| **Text** | `A like 'H%'`, `A not like 'H%'`, `A like 'Hel_o'`, `Contains(A, 'H')`, `StartsWith(A, 'H')`, `EndsWith(A, 'H')`, `IndexOf(A, 'H')`, `Length(A)` |
| **Regular expressions** | `A = /H.*o/`, `Contains(A, /[lL]/)`, other string functions |
| **Collections** | `A[0] = 'Hello'`, `A[?] = 'Hello'` (any), `StartsWith(A[*], 'H')` (all), `Length(A)` |
| **Maths** | `A + 2`, `A - 2`, `A * 2`, `A % 2` |
| **Logic** | `not A`, `A and B`, `A or B` |
| **Grouping** | `A * (B + C)` |
| **Other** | `Has(A)`, `TypeOf(A)` |

### XML `<appSettings>` configuration

Using [_Serilog.Settings.AppSettings](https://github.com/serilog/serilog-settings-appsettings):

```xml
  <add key="serilog:using:FilterExpressions" value="Serilog.Filters.Expressions" />
  <add key="serilog:filter:ByExcluding.expression" value="Name = 'World'" />
```

### JSON `appSettings.json` configuration

Using [_Serilog.Settings.Configuration_](https://github.com/serilog/serilog-settings-configuration):

```json
{
  "Serilog": {
    "Using": ["Serilog.Settings.Configuration"],
    "Filter": [
      {
        "Name": "ByExcluding",
        "Args": {
          "expression": "EndsWith(RequestPath, '/SomeEndpoint')"
        }
      }
    ]
```
