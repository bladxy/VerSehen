using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerSehen.MVVM.Model;

namespace VerSehen.Core
{
    public class StateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(State);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var state = (State)value;
            // Hier definieren Sie, wie das State-Objekt in einen JSON-String serialisiert wird.
            // Zum Beispiel könnten Sie alle Eigenschaften des State-Objekts in einem JSON-Objekt speichern.
            writer.WriteStartObject();
            writer.WritePropertyName("SnakeHeadPositions");
            serializer.Serialize(writer, state.SnakeHeadPositions);
            writer.WritePropertyName("ApplePosition");
            serializer.Serialize(writer, state.ApplePosition);
            writer.WritePropertyName("IsGameOver");
            serializer.Serialize(writer, state.IsGameOver);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            // Hier definieren Sie, wie das State-Objekt aus dem JSON-String deserialisiert wird.
            // Sie würden die Werte aus dem JSON-Objekt extrahieren und verwenden, um ein neues State-Objekt zu erstellen.
            var state = new State();
            state.SnakeHeadPositions = jsonObject["SnakeHeadPositions"].ToObject<List<System.Drawing.Point>>(serializer);
            state.ApplePosition = jsonObject["ApplePosition"].ToObject<System.Drawing.Point>(serializer);
            state.IsGameOver = jsonObject["IsGameOver"].ToObject<bool>(serializer);
            return state;
        }
    }

}
