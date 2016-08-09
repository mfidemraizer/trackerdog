using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Configuration
{
    [ContractClassFor(typeof(IConfigurableTrackableType))]
    public abstract class IConfigurableTrackableTypeContract : IConfigurableTrackableType
    {
        public Type Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IConfigurableTrackableType IncludeProperties(IEnumerable<PropertyInfo> properties)
        {
            Contract.Requires(properties != null && properties.Count() > 0, "Given properties must be a non-null reference and must be an enumerable with at least one property");
            Contract.Ensures(Contract.Result<IConfigurableTrackableType>() != null);
            throw new NotImplementedException();
        }

        public IConfigurableTrackableType IncludeProperties(params PropertyInfo[] properties)
        {
            Contract.Requires(properties != null && properties.Length > 0, "Given properties must be a non-null reference and must be an enumerable with at least one property");
            Contract.Ensures(Contract.Result<IConfigurableTrackableType>() != null);
            throw new NotImplementedException();
        }

        public IConfigurableTrackableType IncludeProperty(PropertyInfo property)
        {
            Contract.Requires(property != null, "A property to include must be non-null reference");
            Contract.Requires(property.GetMethod != null && property.SetMethod != null, $"Given property must implement both a getter and a setter");
            Contract.Requires(property.GetMethod.IsVirtual, $"Given property must be virtual");
            Contract.Ensures(Contract.Result<IConfigurableTrackableType>() != null);

            throw new NotImplementedException();
        }
    }
}
