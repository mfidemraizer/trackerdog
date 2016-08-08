using System;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Marks a property to be ignored from being change-tracked
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DoNotTrackChangesAttribute : Attribute
    {
    }
}