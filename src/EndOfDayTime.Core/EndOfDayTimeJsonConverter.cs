using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EndOfDayTime.Core
{
    /// <summary>
    /// System.Text.Json converter for <see cref="EndOfDayTime"/>.
    /// Serializes and deserializes values as HH:mm strings (00:00–24:00).
    /// </summary>
    public class EndOfDayTimeJsonConverter : JsonConverter<EndOfDayTime>
    {
        /// <inheritdoc/>
        public override EndOfDayTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (EndOfDayTime.TryParse(value, out var result))
                return result;
            throw new JsonException($"'{value}' is not a valid EndOfDayTime.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, EndOfDayTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}