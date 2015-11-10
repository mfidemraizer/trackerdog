namespace TrackerDog.Mixins
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using TrackerDog.Interceptors;

    internal class ChangeTrackableObjectMixin : IChangeTrackableObject
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private readonly Dictionary<string, PropertyInfo> _cachedProperties = new Dictionary<string, PropertyInfo>();

        private Dictionary<string, PropertyInfo> CachedProperties => _cachedProperties;
        public virtual ObjectChangeTracker ChangeTracker { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null)
        {
            ChangeTracker = currentTracker ?? new ObjectChangeTracker(trackableObject);

            foreach (PropertyInfo property in trackableObject.GetType().GetProperties(DefaultBindingFlags))
                if (!property.IsIndexer() && !PropertyInterceptor.ChangeTrackingMembers.Contains(property.Name))
                {
                    PropertyChanged += (sender, e) => TrackProperty(trackableObject, e.PropertyName);

                    if (property.IsList() || property.IsSet())
                        property.MakeTrackable(trackableObject);

                    TrackProperty(trackableObject, property.Name);
                }
        }

        private void TrackProperty(IChangeTrackableObject trackableObject, string propertyName)
        {
            Contract.Assert(ChangeTracker != null);

            PropertyInfo property;

            if (!CachedProperties.TryGetValue(propertyName, out property))
                CachedProperties.Add
                (
                    propertyName,
                    (property = trackableObject.GetType().GetProperty(propertyName, DefaultBindingFlags))
                );

            ChangeTracker.AddOrUpdateTracking(property, trackableObject);
        }

        public void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName)
        {
            PropertyChanged(trackableObject, new PropertyChangedEventArgs(propertyName));
        }
    }
}