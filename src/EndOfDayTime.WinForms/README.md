# EndOfDayTime.WinForms

WinForms control for [EndOfDayTime.Core](https://www.nuget.org/packages/EndOfDayTime.Core) — a `TextBox` that accepts times in the range **00:00–24:00** and integrates with the standard WinForms `ErrorProvider`.

## Installation

```bash
dotnet add package EndOfDayTime.WinForms
```

## Usage

### Basic usage

```csharp
var timeBox = new EndOfDayTimeTextBox();
timeBox.TimeValueChanged += (s, e) =>
{
    Console.WriteLine($"New value: {e}"); // e is EndOfDayTime
};
this.Controls.Add(timeBox);
```

### With ErrorProvider

```csharp
var errorProvider = new ErrorProvider();
errorProvider.Attach(timeBox); // errors shown automatically on invalid input
```

### Programmatic validation

```csharp
if (timeBox.Validate())
{
    var value = timeBox.TimeValue; // guaranteed valid EndOfDayTime
}
else
{
    MessageBox.Show(timeBox.ErrorMessage);
}
```

### Setting a value in code

```csharp
timeBox.TimeValue = new EndOfDayTime(24, 0); // displays "24:00"
timeBox.TimeValue = EndOfDayTime.EndOfDay;   // same result
```

## Behaviour

- Validates on lost focus and on Enter key
- `TimeValue` property updates the displayed text automatically
- `TimeValueChanged` fires when a valid value is confirmed
- `IsValidChanged` fires when validation state changes — useful for enabling/disabling Save buttons
- `ErrorMessage` contains the last validation error
- `MaxLength` is set to 5 automatically (`HH:mm`)
- `PlaceholderText` is set to `HH:mm`

## Requirements

- .NET 8.0-windows+

## License

MIT
