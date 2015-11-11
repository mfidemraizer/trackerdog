namespace TrackerDog.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the default equality comparer to <see cref="ITrackableType"/> implementations. The equality
    /// is based on <see cref="ITrackableType.Type"/> property.
    /// </summary>
    internal sealed class ITrackableTypeEqualityComparer : IEqualityComparer<ITrackableType>
    {
        public bool Equals(ITrackableType x, ITrackableType y) => x.Type == y.Type;
        public int GetHashCode(ITrackableType obj) => obj.Type.AssemblyQualifiedName.GetHashCode();
    }
}