namespace TrackerDog
{
    using System.Collections.Immutable;
    using System.Reflection;

    public interface IObjectPropertyInfo
    {
        IImmutableList<PropertyInfo> PathParts { get; }
        PropertyInfo Property { get; }
        string Path { get; }
    }
}