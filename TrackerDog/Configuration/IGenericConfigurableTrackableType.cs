using System;
using System.Linq.Expressions;

namespace TrackerDog.Configuration
{
    public interface IConfigurableTrackableType<T> : IConfigurableTrackableType
    {
        ITrackableType<T> IncludeProperty(Expression<Func<T, object>> propertySelector);
        ITrackableType<T> IncludeProperties(params Expression<Func<T, object>>[] propertySelectors);
    }
}