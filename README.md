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

### Getting started

Install _Serilog.Filters.Expressions_ from NuGet:

```powershell
Install-Package Serilog.Filters.Expressions
```

Add `Filter.ByIncludingOnly(fiterExpression)` or `Filter.ByExcluding(fiterExpression)` calls to `LoggerConfiguration`.

### Syntax

The syntax is based on SQL, with added support for object structures, arrays, and regular expressions.

| Category | Examples |
| --- | --- |
| **Literals** | `123`, `123.4`, `'Hello'`, `true`, `false`, `null` |
| **Properties** | `A`, `A.B`, `@Level`, `@Timestamp`, `@Exception`, `@Message`, `@MessageTemplate`, `@Properties['A-b-c']` |
| **Comparisons** | `A = B`, `A <> B`, `A > B`, `A >= B`, `A is null`, `A is not null` |
| **Text** | `A like 'H%'`, `A not like 'H%'`, `A like 'Hel_o'`, `Contains(A, 'H')`, `StartsWith(A, 'H')`, `EndsWith(A, 'H')`, `IndexOf(A, 'H')` |
| **Regular expressions** | `A = /H.*o/`, `Contains(A, /[lL]/)`, other string functions |
| **Collections** | `A[0] = 'Hello'`, `A[?] = 'Hello'` (any), `StartsWith(A[*], 'H')` (all) |
| **Maths** | `A + 2`, `A - 2`, `A * 2`, `A % 2` |
| **Logic** | `not A`, `A and B`, `A or B` |
| **Grouping** | `A * (B + C)` |
| **Other** | `Has(A)`, `TypeOf(A)` |
