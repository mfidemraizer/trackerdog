using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using TrackerDog.Configuration;
using TrackerDog.Contracts;
using TrackerDog.Interceptors;

namespace TrackerDog.Mixins
{
    internal class ChangeTrackableObjectMixin : IChangeTrackableObject, IRevertibleChangeTracking
    {
        public ChangeTrackableObjectMixin(IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactoryInternal trackableObjectFactory)
        {
            ChangeTrackingContext.Configuration = configuration;
            TrackableObjectFactory = trackableObjectFactory;
        }

        private readonly static Guid _id = Guid.NewGuid();
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        private ITrackableObjectFactoryInternal TrackableObjectFactory { get; }

        private Dictionary<string, PropertyInfo> CachedProperties { get; } = new Dictionary<string, PropertyInfo>();

        private ObjectChangeTrackingContext ChangeTrackingContext { get; } = new ObjectChangeTrackingContext();

        public ObjectChangeTrackingContext GetChangeTrackingContext() => ChangeTrackingContext;

        public event PropertyChangedEventHandler PropertyChanged;

        internal Guid Id => _id;

        private void ToTrackableCollection(PropertyInfo property, IChangeTrackableObject parentObject)
        {
            Contract.Requires(() => property != null, "Cannot turn the object held by the property because the given property is null");
            Contract.Requires(() => parentObject != null, "A non-null reference to the object owning given property is mandatory");

            if (property.IsEnumerable() && !property.PropertyType.IsArray && ChangeTrackingContext.Configuration.Collections.CanTrack(property.PropertyType))
            {
                object proxiedCollection = TrackableObjectFactory.CreateForCollection(property.GetValue(parentObject), parentObject, property);

                Contract.Assert(() => property.SetMethod != null, $"Property '{property.Name} must implement a setter");
                property.SetValue(parentObject, proxiedCollection);
            }
        }

        public void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null)
        {
            Contract.Requires(() => trackableObject != null, "Given reference must be non-null to be able to track it");

            ChangeTrackingContext.ChangeTracker = currentTracker ?? new ObjectChangeTracker(ChangeTrackingContext.Configuration, TrackableObjectFactory, trackableObject);
            PropertyChanged += (sender, e) => TrackProperty(trackableObject, e.PropertyName);

            ITrackableType trackableType = ChangeTrackingContext.Configuration.GetTrackableType
            (
                trackableObject.GetActualTypeIfTrackable()
            );

            HashSet<PropertyInfo> trackableProperties;

            if (trackableType != null && trackableType.IncludedProperties.Count > 0)
                trackableProperties = new HashSet<PropertyInfo>(trackableType.IncludedProperties);
            else
                trackableProperties = new HashSet<PropertyInfo>
                (
                    trackableObject.GetType().GetProperties(DefaultBindingFlags)
                                        .Where(p => p.CanReadAndWrite())
                );

            IEnumerable<Type> baseTypes = trackableType.Type.GetAllBaseTypes();

            if (baseTypes.Count() > 0)
            {
                foreach (Type baseType in baseTypes)
                    if (ChangeTrackingContext.Configuration.CanTrackType(baseType))
                        foreach (PropertyInfo property in ChangeTrackingContext.Configuration.GetTrackableType(baseType).IncludedProperties)
                            trackableProperties.Add(property);
            }

            foreach (PropertyInfo property in trackableProperties)
                if (!property.IsIndexer() && !PropertyInterceptor.ChangeTrackingMembers.Contains(property.Name))
                {
                    if (property.IsEnumerable())
                    {
                        ToTrackableCollection(property, trackableObject);
                        ChangeTrackingContext.CollectionProperties.Add(property);
                    }

                    TrackProperty(trackableObject, property.Name);
                }
        }

        private void TrackProperty(IChangeTrackableObject trackableObject, string propertyName = null, PropertyInfo property = null)
        {
            Contract.Assert(() => ChangeTrackingContext.ChangeTracker != null);

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

                ChangeTrackingContext.ChangeTracker.AddOrUpdateTracking(cachedProperty ?? property, trackableObject);
            }
            else
                ChangeTrackingContext.ChangeTracker.AddOrUpdateTracking(propertyName, dynamicObject);
        }

        public void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName)
        {
            Contract.Requires(() => trackableObject != null, "A non null reference to a trackable object is mandatory");
            Contract.Requires(() => !string.IsNullOrEmpty(propertyName), "Changed property must have a name");
            Contract.Assert(() => PropertyChanged != null, "This event requires at least an event handler to be raised");

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

        public void RejectChanges() => this.UndoChanges();

        public void AcceptChanges() => ObjectChangeTrackingExtensions.AcceptChanges(this);

        public bool IsChanged => this.GetChangeTracker().ChangedProperties.Count > 0;
    }
}