# EndOfDayTime.AspNet

ASP.NET Core support for [EndOfDayTime.Core](https://www.nuget.org/packages/EndOfDayTime.Core) — a TagHelper and HtmlHelper that render a smart time input accepting values in the range **00:00–24:00**.

## Installation

```bash
dotnet add package EndOfDayTime.AspNet
```

## Setup

Add the script to your layout file (`_Layout.cshtml`) before `</body>`:

```html
<script src="~/_content/EndOfDayTime.AspNet/endofdaytime-input.js"></script>
```

Register the TagHelper in `_ViewImports.cshtml`:

```razor
@addTagHelper *, EndOfDayTime.AspNet
```

## Usage

### TagHelper (Razor Pages / MVC)

```razor
@using EndOfDayTime.AspNet

<eodt-input asp-for="Shift.End" />
```

### HtmlHelper (MVC)

```razor
@using EndOfDayTime.AspNet

@Html.EndOfDayTimeInputFor(m => m.Shift.End)
```

### Model binding

```csharp
public class ShiftDto
{
    public EndOfDayTime Start { get; set; }
    public EndOfDayTime End { get; set; }
}
```

Model binding works automatically via the `TypeConverter` included in `EndOfDayTime.Core`.

## Behaviour

- Accepts only digits — colon inserted automatically after second digit
- Backspace and Delete clear the field
- Paste extracts digits automatically
- `Ctrl+V` supported
- Validates on lost focus

## Requirements

- .NET 8.0+
- ASP.NET Core 8.0+

## License

MIT
