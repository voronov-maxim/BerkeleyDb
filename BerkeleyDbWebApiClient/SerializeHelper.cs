using BerkeleyDbClient;
using BerkeleyDbClient.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BerkeleyDbWebApiClient
{
    internal static class SerializeHelper
    {
        public static JToken CreateJTokenDtoPartialPut(Byte[] key, Byte[] value, int valueLength, int valueOffset)
        {
            using (var jsonWriter = new JTokenWriter())
            {
                jsonWriter.WriteStartObject();

                if (key != null)
                {
                    jsonWriter.WritePropertyName("Key");
                    String base64 = Convert.ToBase64String(key, 0, key.Length);
                    jsonWriter.WriteValue(base64);
                }

                jsonWriter.WritePropertyName("Value");
                {
                    jsonWriter.WriteStartObject();

                    jsonWriter.WritePropertyName("Data");
                    String base64 = Convert.ToBase64String(value, valueOffset, valueLength);
                    jsonWriter.WriteValue(base64);

                    jsonWriter.WritePropertyName("Offset");
                    jsonWriter.WriteValue(valueOffset);

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndObject();
                return jsonWriter.Token;
            }
        }
        public static async Task<BerkeleyDtoGet> GetDataGetAsync(JsonSerializer serializer, HttpContent content)
        {
            using (Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
                return serializer.Deserialize<BerkeleyDtoGet>(jsonReader);
        }
        public static async Task<BerkeleyDbError> GetErrorAsync(JsonSerializer serializer, HttpContent content)
        {
            using (Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
                return (BerkeleyDbError)serializer.Deserialize<int>(jsonReader);
        }
        public static async Task<BerkeleyDtoResult> GetResultAsync(JsonSerializer serializer, HttpContent content)
        {
            using (Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
                return serializer.Deserialize<BerkeleyDtoResult>(jsonReader);
        }
    }
}
