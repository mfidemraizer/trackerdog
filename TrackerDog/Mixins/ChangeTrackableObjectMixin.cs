namespace TrackerDog.Mixins
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using System.Reflection;
    using TrackerDog.Configuration;
    using TrackerDog.Interceptors;

    internal class ChangeTrackableObjectMixin : IChangeTrackableObject
    {
        private readonly static Guid _id = Guid.NewGuid();
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private readonly Dictionary<string, PropertyInfo> _cachedProperties = new Dictionary<string, PropertyInfo>();

        private Dictionary<string, PropertyInfo> CachedProperties => _cachedProperties;
        public virtual ObjectChangeTracker ChangeTracker { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        internal Guid Id => _id;

        public void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null)
        {
            ChangeTracker = currentTracker ?? new ObjectChangeTracker(trackableObject);
            PropertyChanged += (sender, e) => TrackProperty(trackableObject, e.PropertyName);

            ITrackableType trackableType = TrackerDogConfiguration.GetTrackableType
            (
                trackableObject.GetActualTypeIfTrackable()
            );
            IEnumerable<PropertyInfo> trackableProperties;

            if (trackableType != null && trackableType.IncludedProperties.Count > 0)
                trackableProperties = trackableType.IncludedProperties;
            else
                trackableProperties = trackableObject.GetType().GetProperties(DefaultBindingFlags);

            foreach (PropertyInfo property in trackableProperties)
                if (!property.IsIndexer() && !PropertyInterceptor.ChangeTrackingMembers.Contains(property.Name))
                {
                    if (property.IsList() || property.IsSet())
                        property.MakeTrackable(trackableObject);

                    TrackProperty(trackableObject, property.Name);
                }
        }

        private void TrackProperty(IChangeTrackableObject trackableObject, string propertyName = null, PropertyInfo property = null)
        {
            Contract.Assert(ChangeTracker != null);

            DynamicObject dynamicObject = trackableObject as DynamicObject;
            property = property ?? trackableObject.GetType().GetProperty(propertyName, DefaultBindingFlags);

            if (dynamicObject == null || (property != null && property.IsPropertyOfDynamicObject()))
            {
                PropertyInfo cachedProperty;

                if (!CachedProperties.TryGetValue(propertyName, out cachedProperty))
                    CachedProperties.Add
                    (
                        propertyName,
                        property
                    );

                ChangeTracker.AddOrUpdateTracking(cachedProperty ?? property, trackableObject);
            }
            else
                ChangeTracker.AddOrUpdateTracking(propertyName, dynamicObject);
        }

        public void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName)
        {
            PropertyChanged(trackableObject, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            ChangeTrackableObjectMixin mixin = obj as ChangeTrackableObjectMixin;

            if (mixin == null) return false;

            return mixin.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}