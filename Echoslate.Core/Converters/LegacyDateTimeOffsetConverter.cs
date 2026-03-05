using System.Text.Json;
using System.Text.Json.Serialization;

namespace Echoslate.Core.Converters;

public class LegacyDateTimeOffsetConverter : JsonConverter<DateTimeOffset> {
	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType == JsonTokenType.String) {
			string? value = reader.GetString();
			if (string.IsNullOrEmpty(value)) {
				return default;
			}
			if (DateTimeOffset.TryParse(value, out var dto)) {
				return dto;
			}
			if (DateTime.TryParse(value, out var dt)) {
				return new DateTimeOffset(dt, TimeSpan.Zero);
			}
		}
		throw new JsonException($"Unable to convert \"{reader.GetString()}\" to DateTimeOffset");
	}
	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
		writer.WriteStringValue(value.ToString("o"));
	}
}