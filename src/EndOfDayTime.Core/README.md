# EndOfDayTime.Core

A .NET value type that represents a time of day in the range **00:00–24:00**, where `24:00` denotes end-of-day midnight — distinct from `00:00` (start of day).

This concept is defined in ISO 8601 and commonly used in shift scheduling, timesheet applications, broadcasting, and railway timetables.

## Installation

```bash
dotnet add package EndOfDayTime.Core
```

## Usage

### Creating values

```csharp
var start = new EndOfDayTime(9, 0);    // 09:00
var end   = new EndOfDayTime(24, 0);   // 24:00 — end of day
var eod   = EndOfDayTime.EndOfDay;     // 24:00 — static shorthand
```

### Parsing

```csharp
var t = EndOfDayTime.Parse("24:00");

if (EndOfDayTime.TryParse("17:30", out var result))
    Console.WriteLine(result); // 17:30
```

### Formatting

```csharp
var t = new EndOfDayTime(9, 5);
Console.WriteLine(t); // "09:05"
```

### Comparison and arithmetic

```csharp
var start = new EndOfDayTime(9, 0);
var end   = new EndOfDayTime(24, 0);

TimeSpan duration = end - start;  // 15 hours
bool isLast = end > start;        // true
bool isEod  = end.IsEndOfDay;     // true
```

### JSON serialization (System.Text.Json)

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new EndOfDayTimeJsonConverter());

string json    = JsonSerializer.Serialize(new EndOfDayTime(24, 0), options);  // "24:00"
var    parsed  = JsonSerializer.Deserialize<EndOfDayTime>("\"09:30\"", options);
```

### WPF / WinForms / ASP.NET model binding

The `TypeConverter` is registered automatically via `[TypeConverter]` attribute — no configuration needed:

```xml
<!-- WPF -->
<local:EndOfDayTimeTextBox TimeValue="{Binding ShiftEnd}" />
```

```csharp
// ASP.NET Core model binding
public class ShiftDto
{
    public EndOfDayTime End { get; set; }  // "24:00" in JSON binds automatically
}
```

## Notes

- `24:00` and `00:00` are distinct values — the former is end of day, the latter is start of day.
- `24:01` and beyond are invalid and will throw `ArgumentOutOfRangeException`.
- The internal representation is a `short` (total minutes, 0–1440), making it allocation-free.

## License

MIT
