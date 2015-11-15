namespace TrackerDog.Configuration
{
    using System;
    using System.Diagnostics.Contracts;

    public sealed class CollectionImplementation
    {
        private readonly Type _type;
        private readonly Type _changeInterceptor;

        public CollectionImplementation(Type type, Type changeInterceptor)
        {
            Contract.Requires(type != null, "Given collection implementation cannot be a null reference");
            Contract.Requires(type.IsClass && !type.IsAbstract, "Given collection implementation must be a non-abstract class");
            Contract.Requires(type.IsGenericTypeDefinition, "Given collection implementation must be a generic type definition");
            Contract.Requires(changeInterceptor != null, "A collection change interceptor type is mandatory");
            Contract.Requires(changeInterceptor.IsGenericType && changeInterceptor.IsGenericTypeDefinition, "Given collection change interceptor must be a generic type definition");

            _type = type;
            _changeInterceptor = changeInterceptor;
        }

        public Type Type => _type;
        public Type ChangeInterceptor => _changeInterceptor;
    }
}
