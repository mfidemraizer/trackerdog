namespace TrackerDog
{
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y) =>
            x.DeclaringType == y.DeclaringType
            && x.Name == y.Name;

        public int GetHashCode(PropertyInfo obj) =>
            (obj.DeclaringType.AssemblyQualifiedName.GetHashCode() + obj.Name.GetHashCode());
    }
}