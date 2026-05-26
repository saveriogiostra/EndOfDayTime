# EndOfDayTime.WebForms

ASP.NET WebForms support for [EndOfDayTime.Core](https://www.nuget.org/packages/EndOfDayTime.Core) — a server control that renders a smart time input accepting values in the range **00:00–24:00**, including end-of-day midnight.

## Requirements

- .NET Framework 4.8
- ASP.NET WebForms

## Installation

```bash
dotnet add package EndOfDayTime.WebForms
```

## Setup

Register the control namespace in each page or control that uses it:

```aspx
<%@ Register Assembly="EndOfDayTime.WebForms" Namespace="EndOfDayTime.WebForms" TagPrefix="eodt" %>
```

Or globally in `Web.config` to avoid repeating the registration on every page:

```xml
<system.web>
  <pages>
    <controls>
      <add assembly="EndOfDayTime.WebForms"
           namespace="EndOfDayTime.WebForms"
           tagPrefix="eodt" />
    </controls>
  </pages>
</system.web>
```

The JavaScript is loaded automatically via WebResource — no manual script tag needed.

## Usage

### Markup

```aspx
<eodt:EndOfDayTimeTextBox ID="ShiftEnd" runat="server" />
```

### Code-behind

```csharp
using EodtCore = EndOfDayTime.Core;

protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
        ShiftEnd.TimeValue = new EodtCore.EndOfDayTime(17, 30);
}

protected void SaveButton_Click(object sender, EventArgs e)
{
    if (ShiftEnd.IsValid)
    {
        var value = ShiftEnd.TimeValue;
        // use value
    }
}
```

### TimeValueChanged event

```csharp
private void OnShiftEndChanged(object sender, EventArgs e)
{
    var value = ShiftEnd.TimeValue;
}
```

### Optional CSS class

```aspx
<eodt:EndOfDayTimeTextBox ID="ShiftEnd" runat="server" InputCssClass="my-time-input" />
```

## Behaviour

- Accepts only digits — colon inserted automatically after the second digit
- Backspace and Delete clear the entire field
- Paste extracts digits automatically — both context menu and Ctrl+V
- ViewState supported — value survives postbacks
- PostBack data handled automatically via IPostBackDataHandler
- Client script loaded automatically via WebResource — no manual setup needed

## License

MIT
