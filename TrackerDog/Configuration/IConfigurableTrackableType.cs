using System;
using System.Reflection;

namespace TrackerDog.Configuration
{
    public interface IConfigurableTrackableType
    {
        Type Type { get; }

        ITrackableType IncludeProperty(PropertyInfo property);
        ITrackableType IncludeProperties(params PropertyInfo[] properties);
    }
}
