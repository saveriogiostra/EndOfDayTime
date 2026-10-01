using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EndOfDayTime.Core
{
    /// <summary>
    /// System.Text.Json converter for <see cref="EndOfDayTime"/>.
    /// Serializes and deserializes values as HH:mm strings (00:00–24:00).
    /// Applied to <see cref="EndOfDayTime"/> by attribute, so no registration is needed.
    /// </summary>
    public class EndOfDayTimeJsonConverter : JsonConverter<EndOfDayTime>
    {
        /// <inheritdoc/>
        public override EndOfDayTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected a string in HH:mm format (00:00–24:00) for EndOfDayTime.");

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