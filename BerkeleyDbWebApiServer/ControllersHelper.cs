using BerkeleyDbClient;
using Newtonsoft.Json.Linq;
using System;

namespace BerkeleyDbWebApiServer
{
    internal static class ControllersHelper
    {
        public static JToken CreateJTokenObject(BerkeleyDbError error, Byte[] key, Byte[] value, int keySize, int valueSize)
        {
            using (var jsonWriter = new JTokenWriter())
            {
                jsonWriter.WriteStartObject();

                if (error != 0)
                {
                    jsonWriter.WritePropertyName("ErrorCode");
                    jsonWriter.WriteValue((int)error);
                }

                if (key != null)
                {
                    jsonWriter.WritePropertyName("Key");
                    String base64 = Convert.ToBase64String(key, 0, keySize);
                    jsonWriter.WriteValue(base64);
                }

                if (value != null)
                {
                    jsonWriter.WritePropertyName("Value");
                    String base64 = Convert.ToBase64String(value, 0, valueSize);
                    jsonWriter.WriteValue(base64);
                }

                jsonWriter.WriteEndObject();
                return jsonWriter.Token;
            }
        }
    }
}
