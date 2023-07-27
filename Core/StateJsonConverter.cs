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
            writer.WriteStartObject();
            writer.WritePropertyName("SnakeHeadPosition");
            serializer.Serialize(writer, state.SnakeHeadPosition);
            writer.WritePropertyName("ApplePosition");
            serializer.Serialize(writer, state.ApplePosition);
            writer.WritePropertyName("IsGameOver");
            serializer.Serialize(writer, state.IsGameOver);
            writer.WritePropertyName("SnakeBodyPoints");  
            serializer.Serialize(writer, state.SnakeBodyPoints);  
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var state = new State();
            state.SnakeHeadPosition = jsonObject["SnakeHeadPosition"].ToObject<System.Drawing.Point>(serializer);
            state.ApplePosition = jsonObject["ApplePosition"].ToObject<System.Drawing.Point>(serializer);
            state.IsGameOver = jsonObject["IsGameOver"].ToObject<bool>(serializer);
            state.SnakeBodyPoints = jsonObject["SnakeBodyPoints"].ToObject<List<System.Drawing.Point>>(serializer);  
            return state;
        }

    }

}
