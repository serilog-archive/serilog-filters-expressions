# Serilog.Filters.Expressions

Expression-based event filtering for [Serilog](https://serilog.net).

```csharp
var expr = "@Level = 'Information' and AppId is not null and Items[?] like 'C%'";

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("AppId", 10)
    .Filter.ByIncludingOnly(expr)
    .WriteTo.LiterateConsole()
    .CreateLogger();

// Printed
Log.Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
Log.Information("Cart contains {@Items}", new[] { "Peanuts", "Chocolate" });

// Not printed
Log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
Log.Information("Cart contains {@Items}", new[] { "Apricots" });

Log.CloseAndFlush();
```

The syntax is based on SQL, with added support for object structures, arrays, and regular expressions.

### Getting started

Install _Serilog.Filters.Expressions_ from NuGet:

```powershell
Install-Package Serilog.Filters.Expressions
```

Add `Filter.ByIncludingOnly(fiterExpression)` or `Filter.ByExcluding(fiterExpression)` calls to `LoggerConfiguration`.

### Syntax

**Literals:** `123`, `123.4`, `'Hello'`, `true`, `false`, `null`.

**Properties:** `A`, `A.B`, `A[C]`, `@Level`, `@Timestamp`, `@Exception`, `@Properties['A-b-c']`.

**Comparisons:** `A = B`, `A <> B`, `A > B`, `A >= B`.

**Text:** `A like 'H%'`, `A like 'Hel_o'`, `Contains(A, 'H')`, `StartsWith(A, 'H')`, `EndsWith(A, 'H')`, `IndexOf(A, 'H')`.

**Regular expressions:** _TBC_

**Collections:** _TBC_

**Maths:** _TBC_
