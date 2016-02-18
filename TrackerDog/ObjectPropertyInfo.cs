namespace TrackerDog
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay("{Path}")]
    internal sealed class ObjectPropertyInfo : IEquatable<ObjectPropertyInfo>, IObjectPropertyInfo
    {
        public IList<PropertyInfo> PathParts { get; private set; } = new List<PropertyInfo>();

        public PropertyInfo Property => PathParts.Last();

        public string Path => string.Join(".", PathParts.Select(p => p.Name));

        IImmutableList<PropertyInfo> IObjectPropertyInfo.PathParts => PathParts.ToImmutableList();

        string IObjectPropertyInfo.Path => Path;

        public ObjectPropertyInfo Clone() => new ObjectPropertyInfo() { PathParts = PathParts.ToList() };

        public bool Equals(ObjectPropertyInfo other)
        {
            if (ReferenceEquals(other, null))
                return false;
            else if (other.Property == Property && other.PathParts.Intersect(PathParts).Count() == other.PathParts.Count)
                return true;
            else return false;
        }
    }
}