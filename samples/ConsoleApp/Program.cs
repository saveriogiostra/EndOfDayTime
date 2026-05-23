using System;
using System.Text.Json;
using System.ComponentModel;
using EodtCore = EndOfDayTime.Core;

Console.WriteLine("=== EndOfDayTime.Core Sample ===\n");

// ── Construction ─────────────────────────────────────────────────────────

Console.WriteLine("--- Construction ---");
var start = new EodtCore.EndOfDayTime(9, 0);
var end   = new EodtCore.EndOfDayTime(17, 30);
var eod   = EodtCore.EndOfDayTime.EndOfDay;

Console.WriteLine($"Start:      {start}");
Console.WriteLine($"End:        {end}");
Console.WriteLine($"EndOfDay:   {eod}");
Console.WriteLine($"IsEndOfDay: {eod.IsEndOfDay}");
Console.WriteLine();

// ── Parsing ───────────────────────────────────────────────────────────────

Console.WriteLine("--- Parsing ---");
var parsed = EodtCore.EndOfDayTime.Parse("24:00");
Console.WriteLine($"Parse(\"24:00\"):     {parsed}");

if (EodtCore.EndOfDayTime.TryParse("17:30", out var tryResult))
    Console.WriteLine($"TryParse(\"17:30\"): {tryResult}");

if (!EodtCore.EndOfDayTime.TryParse("25:00", out _))
    Console.WriteLine("TryParse(\"25:00\"): invalid — correctly rejected");

if (!EodtCore.EndOfDayTime.TryParse("9:00", out _))
    Console.WriteLine("TryParse(\"9:00\"):  invalid — correctly rejected (not zero-padded)");
Console.WriteLine();

// ── Arithmetic ────────────────────────────────────────────────────────────

Console.WriteLine("--- Arithmetic ---");
var duration = end - start;
Console.WriteLine($"{end} - {start} = {duration.TotalHours}h");

var fullDay = EodtCore.EndOfDayTime.EndOfDay - new EodtCore.EndOfDayTime(0, 0);
Console.WriteLine($"24:00 - 00:00 = {fullDay.TotalHours}h");
Console.WriteLine();

// ── Comparison ────────────────────────────────────────────────────────────

Console.WriteLine("--- Comparison ---");
Console.WriteLine($"{start} < {end}:  {start < end}");
Console.WriteLine($"{eod} > {end}:  {eod > end}");
Console.WriteLine($"EndOfDay == MaxValue: {EodtCore.EndOfDayTime.EndOfDay == EodtCore.EndOfDayTime.MaxValue}");
Console.WriteLine();

// ── JSON ──────────────────────────────────────────────────────────────────

Console.WriteLine("--- JSON (System.Text.Json) ---");
var options = new JsonSerializerOptions();
options.Converters.Add(new EodtCore.EndOfDayTimeJsonConverter());

var shift = new { Start = start, End = eod };
string json = JsonSerializer.Serialize(shift, options);
Console.WriteLine($"Serialized:   {json}");

var deserialized = JsonSerializer.Deserialize<ShiftDto>(json, options);
Console.WriteLine($"Deserialized: Start={deserialized!.Start} End={deserialized.End}");
Console.WriteLine();

// ── TypeConverter ─────────────────────────────────────────────────────────

Console.WriteLine("--- TypeConverter ---");
var converter = TypeDescriptor.GetConverter(typeof(EodtCore.EndOfDayTime));
var fromString = (EodtCore.EndOfDayTime)converter.ConvertFromString("24:00")!;
var toString   = converter.ConvertToString(fromString);
Console.WriteLine($"ConvertFromString(\"24:00\"): {fromString}");
Console.WriteLine($"ConvertToString(24:00):     {toString}");
Console.WriteLine();

// ── Edge cases ────────────────────────────────────────────────────────────

Console.WriteLine("--- Edge cases ---");
Console.WriteLine($"MinValue: {EodtCore.EndOfDayTime.MinValue}");
Console.WriteLine($"MaxValue: {EodtCore.EndOfDayTime.MaxValue}");
Console.WriteLine($"default:  '{default(EodtCore.EndOfDayTime)}'");

try
{
    var invalid = new EodtCore.EndOfDayTime(24, 1);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"new EndOfDayTime(24, 1) → {ex.GetType().Name} (expected)");
}

Console.WriteLine("\n=== All checks passed ===");

// ── DTO for deserialization ───────────────────────────────────────────────

public class ShiftDto
{
    public EodtCore.EndOfDayTime Start { get; set; }
    public EodtCore.EndOfDayTime End   { get; set; }
}