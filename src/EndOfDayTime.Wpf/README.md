# EndOfDayTime.Wpf

WPF control for [EndOfDayTime.Core](https://www.nuget.org/packages/EndOfDayTime.Core) — a `TextBox` that accepts times in the range **00:00–24:00** and exposes an `EndOfDayTime` dependency property for data binding.

## Installation

```bash
dotnet add package EndOfDayTime.Wpf
```

## Usage

### XAML namespace

```xml
xmlns:eodt="clr-namespace:EndOfDayTime.Wpf;assembly=EndOfDayTime.Wpf"
```

### Basic binding

```xml
<eodt:EndOfDayTimeTextBox TimeValue="{Binding ShiftEnd}" />
```

### With ValidationRule

```xml
<TextBox>
    <TextBox.Text>
        <Binding Path="ShiftEnd" UpdateSourceTrigger="LostFocus">
            <Binding.ValidationRules>
                <eodt:EndOfDayTimeValidationRule />
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```

### Events

```csharp
myTimeBox.TimeValueChanged += (s, e) =>
{
    Console.WriteLine($"Changed from {e.OldValue} to {e.NewValue}");
};
```

### Programmatic validation

```csharp
if (myTimeBox.Validate())
{
    var value = myTimeBox.TimeValue; // guaranteed valid EndOfDayTime
}
else
{
    // myTimeBox.IsValid == false
}
```

## Behaviour

- Validates on lost focus and on Enter key
- `TimeValue` dependency property supports two-way binding
- `TimeValueChanged` is a bubbling routed event
- `MaxLength` is set to 5 automatically (`HH:mm`)

## Requirements

- .NET 8.0-windows+

## License

MIT
