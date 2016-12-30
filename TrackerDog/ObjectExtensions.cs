using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TrackerDog.Serialization.Json;

namespace TrackerDog
{
    internal static class ObjectExtensions
    {
        public static object CloneIt(this object some, Type type)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.ContractResolver = new CustomObjectContractResolver();

            return JObject.FromObject(some, serializer).ToObject(type, serializer);
        }
    }
}