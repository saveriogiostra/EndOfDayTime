# EndOfDayTime.EntityFramework

EF Core support for [EndOfDayTime.Core](https://www.nuget.org/packages/EndOfDayTime.Core) — persists `EndOfDayTime` values as `smallint` (minutes) in the database.

## Installation

```bash
dotnet add package EndOfDayTime.EntityFramework
```

## Usage

### Per-property configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<WorkShift>(entity =>
    {
        entity.Property(s => s.Start).HasEndOfDayTimeConverter();
        entity.Property(s => s.End).HasEndOfDayTimeConverter();
    });
}
```

### Automatic configuration (all properties)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyEndOfDayTimeConverter();
}
```

This scans all entities and applies the converter to every `EndOfDayTime` property automatically.

### Example model

```csharp
public class WorkShift
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EndOfDayTime Start { get; set; }
    public EndOfDayTime End { get; set; }
}
```

### Database representation

Values are stored as `smallint` (0–1440 minutes). `24:00` is stored as `1440`.

| C# value | DB value |
| -------- | -------- |
| `00:00`  | `0`      |
| `09:30`  | `570`    |
| `17:00`  | `1020`   |
| `24:00`  | `1440`   |

## Requirements

- .NET 8.0+
- Microsoft.EntityFrameworkCore 8.x

## License

MIT
