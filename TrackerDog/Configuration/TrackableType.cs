using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using TrackerDog.Contracts;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Represents the default implementation to a fluent trackable type configuration.
    /// </summary>
    internal class TrackableType : ITrackableType, IConfigurableTrackableType
    {
        private readonly ISet<PropertyInfo> _includedProperties = new HashSet<PropertyInfo>(new PropertyInfoEqualityComparer());
        private readonly Type _type;
        private readonly Lazy<IImmutableSet<IObjectPropertyInfo>> _objectPaths;

        public TrackableType(IObjectChangeTrackingConfiguration configuration, Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();

            Contract.Assert(() => (typeInfo.IsClass && !typeInfo.IsSealed) || typeInfo.IsInterface, $"Given type '{type.AssemblyQualifiedName}' must be either a non-sealed class or an interface");

            _type = type;
            _objectPaths = new Lazy<IImmutableSet<IObjectPropertyInfo>>
            (
                () => Type.BuildAllPropertyPaths(p => configuration.CanTrackType(p.DeclaringType))
                            .Cast<IObjectPropertyInfo>()
                            .ToImmutableHashSet()
            );
        }

        public Type Type => _type;
        public IImmutableSet<IObjectPropertyInfo> ObjectPaths => _objectPaths.Value;
        public IImmutableSet<PropertyInfo> IncludedProperties => _includedProperties.ToImmutableHashSet(new PropertyInfoEqualityComparer());

        public IConfigurableTrackableType IncludeProperty(PropertyInfo property)
        {
            Contract.Requires(() => property != null, "A property to include must be non-null reference");
            Contract.Requires(() => property.GetMethod != null && property.SetMethod != null, $"Given property must implement both a getter and a setter");
            Contract.Requires(() => property.GetMethod.IsVirtual, $"Given property must be virtual");
            Contract.Assert(() => _includedProperties.Add(property), "Property must be included once");

            return this;
        }

        public IConfigurableTrackableType IncludeProperties(params PropertyInfo[] properties)
        {
            Contract.Requires(() => properties != null && properties.Count() > 0, "Given properties must be a non-null reference and must be an enumerable with at least one property");

            return IncludeProperties((IEnumerable<PropertyInfo>)properties);
        }

        public IConfigurableTrackableType IncludeProperties(IEnumerable<PropertyInfo> properties)
        {
            Contract.Requires(() => properties != null && properties.Count() > 0, "Given properties must be a non-null reference and must be an enumerable with at least one property");

            foreach (PropertyInfo property in properties)
                IncludeProperty(property);

            return this;
        }
    }
}