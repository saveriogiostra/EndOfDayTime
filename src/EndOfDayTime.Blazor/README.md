# EndOfDayTime.Blazor

Blazor component for [EndOfDayTime.Core](https://www.nuget.org/packages/EndOfDayTime.Core) — a text input that accepts times in the range **00:00–24:00**, fully integrated with Blazor's `EditForm` and validation system.

## Installation

```bash
dotnet add package EndOfDayTime.Blazor
```

## Usage

### Basic binding

```razor
<EndOfDayTimeInput @bind-Value="model.End" />
```

### Inside an EditForm with validation

```razor
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />

    <label>Shift end</label>
    <EndOfDayTimeInput @bind-Value="model.End" />
    <ValidationMessage For="@(() => model.End)" />

    <button type="submit">Save</button>
</EditForm>

@code {
    private ShiftModel model = new();

    private void HandleSubmit()
    {
        // model.End is a valid EndOfDayTime here
    }

    public class ShiftModel
    {
        public EndOfDayTime End { get; set; }
    }
}
```

### Behaviour

- Accepts any value in `HH:mm` format (00:00–24:00)
- Shows an inline error message on blur if the value is invalid
- Integrates with `EditForm` validation — `ValidationMessage` works as expected
- Empty input is allowed (maps to `default(EndOfDayTime)`)

## Requirements

- .NET 8.0+
- Microsoft.AspNetCore.Components.Web 8.x

## License

MIT
