using System.Collections.Immutable;
using System.Reflection;

namespace TrackerDog
{
    /// <summary>
    /// Defines a type which holds metadata about a given type property.
    /// </summary>
    public interface IObjectPropertyInfo
    {
        /// <summary>
        /// Gets a list where each item is the owner association for the next one
        /// </summary>
        IImmutableList<PropertyInfo> PathParts { get; }

        /// <summary>
        /// Gets a reference to the last property in the path.
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Gets a string, dot-separated representation of <see cref="PathParts"/>
        /// </summary>
        string Path { get; }
    }
}