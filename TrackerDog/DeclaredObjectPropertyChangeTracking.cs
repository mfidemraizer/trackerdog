using Castle.DynamicProxy;
using TrackerDog.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace TrackerDog
{
    [DebuggerDisplay("{Property.DeclaringType}.{Property.Name} = {CurrentValue} (Has changed? {HasChanged})")]
    internal sealed class DeclaredObjectPropertyChangeTracking : IDeclaredObjectPropertyChangeTracking
    {
        private object _oldValue;
        private object _currentValue;
        private IEnumerable _oldCollectionValue;
        private IEnumerable _currentCollectionValue;

        public DeclaredObjectPropertyChangeTracking(IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactoryInternal trackableObjectFactory, ObjectChangeTracker tracker, object targetObject, PropertyInfo ownerProperty, PropertyInfo property, object currentValue)
        {
            Configuration = configuration;
            TrackableObjectFactory = trackableObjectFactory;
            Tracker = tracker;
            TargetObject = targetObject;
            Property = property;
            OldValue = currentValue;
            CurrentValue = currentValue;
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }

        private ITrackableObjectFactoryInternal TrackableObjectFactory { get; }

        public ObjectChangeTracker Tracker { get; }
        IObjectChangeTracker IObjectPropertyChangeTracking.Tracker => Tracker;
        public object TargetObject { get; }
        public PropertyInfo Property { get; private set; }
        public string PropertyName => Property.Name;
        private bool CollectionItemsAreTrackable { get; set; }

        public object OldValue
        {
            get { return _oldValue; }
            set
            {
                _oldValue = value;

                if (_oldValue != null && Configuration.Collections.CanTrack(_oldValue.GetType()))
                {
                    IEnumerable enumerable = _oldValue as IEnumerable;

                    if (enumerable != null)
                    {
                        _oldCollectionValue = enumerable.CloneEnumerable(Configuration);

                        if (_oldValue is IProxyTargetAccessor)
                            _oldCollectionValue = (IEnumerable)TrackableObjectFactory.CreateForCollection
                            (
                                _oldCollectionValue, (IChangeTrackableObject)TargetObject, Property
                            );

                        _oldValue = _oldCollectionValue;

                        CollectionItemsAreTrackable = Configuration.CanTrackType(_oldValue.GetCollectionItemType());
                    }
                }
            }
        }

        public object CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                if (!(value is string))
                {
                    _currentCollectionValue = value as IEnumerable;
                }
            }
        }

        private IEnumerable OldCollectionValue => _oldCollectionValue;
        private IEnumerable CurrentCollectionValue => _currentCollectionValue;
        public bool IsCollection => OldCollectionValue != null;

        public bool HasChanged
        {
            get
            {
                if (!IsCollection)
                {
                    if (CurrentValue == null && OldValue == null)
                        return false;
                    else
                    {
                        var result = CurrentValue?.Equals(OldValue);

                        return result != null && !(bool)result;
                    }
                }
                else
                {
                    IEnumerable<object> oldCollection = OldCollectionValue.Cast<object>();
                    IEnumerable<object> currentCollection = CurrentCollectionValue.Cast<object>();

                    if (oldCollection.Count() != currentCollection.Count())
                        return true;
                    else if (CollectionItemsAreTrackable)
                        return currentCollection.Any(o =>
                        {
                            IChangeTrackableObject trackable = o as IChangeTrackableObject;

                            if (trackable != null) return trackable.GetChangeTrackingContext().ChangeTracker.ChangedProperties.Count > 0;
                            else return false;
                        });
                    else
                        return currentCollection.Intersect(oldCollection).Count() != currentCollection.Count();
                }
            }
        }

        public override bool Equals(object obj)
        {
            IDeclaredObjectPropertyChangeTracking declared = obj as IDeclaredObjectPropertyChangeTracking;

            if (declared != null) return Equals(declared);
            else return Equals(obj as IObjectPropertyChangeTracking);
        }

        public bool Equals(IDeclaredObjectPropertyChangeTracking other) =>
            other != null && other.Property == Property;

        public bool Equals(IObjectPropertyChangeTracking other) =>
            other != null && other.PropertyName == PropertyName;

        public override int GetHashCode() =>
            Property.Name.GetHashCode() + Property.DeclaringType.AssemblyQualifiedName.GetHashCode();
    }
}