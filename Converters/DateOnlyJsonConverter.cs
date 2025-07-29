using System.Text.Json;
using System.Text.Json.Serialization;

namespace FaizHesaplamaAPI.Converters
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private readonly string[] _formats = new[] { "dd.MM.yyyy", "yyyy-MM-dd" };

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (DateOnly.TryParseExact(value, _formats, out var date))
            {
                return date;
            }
            throw new JsonException($"'{value}' is not a valid date format. Use 'dd.MM.yyyy' or 'yyyy-MM-dd'.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }

    public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        private readonly string[] _formats = new[] { "dd.MM.yyyy", "yyyy-MM-dd" };

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value)) return null;

            if (DateOnly.TryParseExact(value, _formats, out var date))
            {
                return date;
            }
            throw new JsonException($"'{value}' is not a valid date format. Use 'dd.MM.yyyy' or 'yyyy-MM-dd'.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-dd"));
        }
    }
}










//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace FaizHesaplamaAPI.Converters
//{
//    public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
//    {
//        private const string Format = "yyyy-MM-dd";

//        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        {
//            var value = reader.GetString();

//            if (string.IsNullOrEmpty(value))
//            {
//                return null;
//            }

//            if (DateOnly.TryParse(value, out var date))
//            {
//                return date;
//            }

//            throw new JsonException($"Unable to parse '{value}' as DateOnly.");
//        }

//        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
//        {
//            if (value.HasValue)
//            {
//                writer.WriteStringValue(value.Value.ToString(Format));
//            }
//            else
//            {
//                writer.WriteNullValue();
//            }
//        }
//    }
//}