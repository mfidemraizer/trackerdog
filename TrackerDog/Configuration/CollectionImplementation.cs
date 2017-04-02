using System;
using System.Reflection;
using TrackerDog.Contracts;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Represents a collection implementation information
    /// </summary>
    public sealed class CollectionImplementation
    {
        private readonly Type _type;
        private readonly Type _changeInterceptor;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="type">The collection implementation type</param>
        /// <param name="changeInterceptorType">The collection change interceptor implementation type</param>
        public CollectionImplementation(Type type, Type changeInterceptorType)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            TypeInfo changeInterceptorTypeInfo = changeInterceptorType.GetTypeInfo();

            Contract.Requires(() => type != null, "Given collection implementation cannot be a null reference");
            Contract.Requires(() => typeInfo.IsClass && !typeInfo.IsAbstract, "Given collection implementation must be a non-abstract class");
            Contract.Requires(() => typeInfo.IsGenericTypeDefinition, "Given collection implementation must be a generic type definition");
            Contract.Requires(() => changeInterceptorTypeInfo != null, "A collection change interceptor type is mandatory");
            Contract.Requires(() => changeInterceptorTypeInfo.IsGenericType && changeInterceptorTypeInfo.IsGenericTypeDefinition, "Given collection change interceptor must be a generic type definition");

            _type = type;
            _changeInterceptor = changeInterceptorType;
        }

        /// <summary>
        /// Gets the collection implementation type
        /// </summary>
        public Type Type => _type;

        /// <summary>
        /// Gets the collection change interceptor implementation type
        /// </summary>
        public Type ChangeInterceptor => _changeInterceptor;
    }
}