using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Configuration
{
    internal sealed class AttributeConfigurationBuilder
    {
        public AttributeConfigurationBuilder(IObjectChangeTrackingConfiguration config)
        {
            Configuration = config;
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }

        public void ConfigureType(IConfigurableTrackableType trackableType)
        {
            Contract.Requires(trackableType != null);

            if (trackableType.Type.GetCustomAttribute<ChangeTrackableAttribute>() != null)
                trackableType.IncludeProperties(GetTrackableProperties(trackableType.Type));
        }

        private IEnumerable<PropertyInfo> GetTrackableProperties(Type type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<IEnumerable<PropertyInfo>>() != null);

            IEnumerable<PropertyInfo> allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            IEnumerable<PropertyInfo> trackableProperties = allProperties.Where(p => p.GetCustomAttribute<ChangeTrackableAttribute>() != null);
            IEnumerable<PropertyInfo> nonTrackableProperties = allProperties.Where(p => p.GetCustomAttribute<DoNotTrackChangesAttribute>() != null);

            if (nonTrackableProperties.Count() > 0)
                return allProperties.Except(nonTrackableProperties);
            else if (trackableProperties.Count() > 0)
                return trackableProperties;
            else
                return allProperties;
        }
    }
}
