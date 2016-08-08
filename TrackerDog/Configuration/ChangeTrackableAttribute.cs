using System;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Either marks a class, interface or a property to be change-trackable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ChangeTrackableAttribute : Attribute
    {
    }
}