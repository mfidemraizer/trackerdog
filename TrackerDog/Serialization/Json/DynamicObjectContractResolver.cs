using Newtonsoft.Json.Serialization;
using System;
using System.Dynamic;

namespace TrackerDog.Serialization.Json
{
    internal class DynamicObjectContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(objectType))
            {
                return CreateObjectContract(objectType);
            }

            return base.CreateContract(objectType);
        }

    }
}