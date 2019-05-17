using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public enum LibraryManager { AerieHub, ArchScan };

    public enum HealthScore { DoingOk, Empty, Happy };

    public partial class Company
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("note")]
        public object Note { get; set; }

        [JsonProperty("domains")]
        public string[] Domains { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("custom_fields")]
        public CompanyCustomFields CustomFields { get; set; }

        [JsonProperty("health_score")]
        public HealthScore HealthScore { get; set; }

        [JsonProperty("account_tier")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long AccountTier { get; set; }

        [JsonProperty("renewal_date")]
        public DateTimeOffset? RenewalDate { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }
    }

    public partial class CompanyCustomFields
    {
        [JsonProperty("library_id")]
        public long? LibraryId { get; set; }

        [JsonProperty("library_manager")]
        public LibraryManager? LibraryManager { get; set; }

        [JsonProperty("documents_module")]
        public bool DocumentsModule { get; set; }

        [JsonProperty("compliance_module")]
        public bool? ComplianceModule { get; set; }

        [JsonProperty("space_management")]
        public bool? SpaceManagement { get; set; }

        [JsonProperty("training_amp_certification")]
        public bool? TrainingAmpCertification { get; set; }
    }

    public partial class Company
    {
        public static Company[] FromJson(string json) => JsonConvert.DeserializeObject<Company[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Company[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                LibraryManagerConverter.Singleton,
                HealthScoreConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class LibraryManagerConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(LibraryManager) || t == typeof(LibraryManager?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "AerieHub":
                    return LibraryManager.AerieHub;

                case "archSCAN":
                    return LibraryManager.ArchScan;
            }
            throw new Exception("Cannot unmarshal type LibraryManager");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (LibraryManager)untypedValue;
            switch (value)
            {
                case LibraryManager.AerieHub:
                    serializer.Serialize(writer, "AerieHub");
                    return;

                case LibraryManager.ArchScan:
                    serializer.Serialize(writer, "archSCAN");
                    return;
            }
            throw new Exception("Cannot marshal type LibraryManager");
        }

        public static readonly LibraryManagerConverter Singleton = new LibraryManagerConverter();
    }

    internal class HealthScoreConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(HealthScore) || t == typeof(HealthScore?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "":
                    return HealthScore.Empty;

                case "Doing OK":
                    return HealthScore.DoingOk;

                case "Happy":
                    return HealthScore.Happy;
            }
            throw new Exception("Cannot unmarshal type HealthScore");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (HealthScore)untypedValue;
            switch (value)
            {
                case HealthScore.Empty:
                    serializer.Serialize(writer, "");
                    return;

                case HealthScore.DoingOk:
                    serializer.Serialize(writer, "Doing OK");
                    return;

                case HealthScore.Happy:
                    serializer.Serialize(writer, "Happy");
                    return;
            }
            throw new Exception("Cannot marshal type HealthScore");
        }

        public static readonly HealthScoreConverter Singleton = new HealthScoreConverter();
    }
}