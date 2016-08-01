using System.Reflection;

namespace TrackerDog.Configuration
{
    internal interface ICanConfigureTrackableType<out TTrackableType>
        where TTrackableType : ITrackableType
    {
        TTrackableType IncludeProperty(PropertyInfo property);
        TTrackableType IncludeProperties(params PropertyInfo[] properties);
    }
}
