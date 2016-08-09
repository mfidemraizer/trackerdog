using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace TrackerDog.Configuration
{
    [ContractClassFor(typeof(ITrackableType))]
    public abstract class ITrackableTypeContract : ITrackableType
    {
        public IImmutableSet<PropertyInfo> IncludedProperties
        {
            get
            {
                Contract.Ensures(Contract.Result<IImmutableSet<PropertyInfo>>() != null, "'IncludedProperties' cannot be null");

                throw new NotImplementedException();
            }
        }

        public IImmutableSet<IObjectPropertyInfo> ObjectPaths
        {
            get
            {
                Contract.Ensures(Contract.Result<IImmutableSet<IObjectPropertyInfo>>() != null, "'ObjectPaths' cannot be null");

                throw new NotImplementedException();
            }
        }

        public Type Type
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null, $"'Type' cannot be null");

                throw new NotImplementedException();
            }
        }
    }
}