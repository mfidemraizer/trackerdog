namespace TrackerDog
{
    using Castle.DynamicProxy;
    using Configuration;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay("{Property.DeclaringType.Name}.{Property.Name} = {CurrentValue} (Has changed? {HasChanged})")]
    internal sealed class DeclaredObjectPropertyChangeTracking : IDeclaredObjectPropertyChangeTracking
    {
        private readonly ObjectChangeTracker _tracker;
        private readonly object _targetObject;
        private object _oldValue;
        private object _currentValue;
        private IEnumerable _oldCollectionValue;
        private IEnumerable _currentCollectionValue;
        private readonly PropertyInfo _ownerProperty;

        public DeclaredObjectPropertyChangeTracking(ObjectChangeTracker tracker, object targetObject, PropertyInfo ownerProperty, PropertyInfo property, object currentValue)
        {
            _tracker = tracker;
            _targetObject = targetObject;
            _ownerProperty = ownerProperty;
            Property = property;
            OldValue = currentValue;
            CurrentValue = currentValue;
        }

        public ObjectChangeTracker Tracker => _tracker;
        IObjectChangeTracker IObjectPropertyChangeTracking.Tracker => Tracker;
        public object TargetObject => _targetObject;
        public PropertyInfo OwnerProperty => _ownerProperty;
        public PropertyInfo Property { get; private set; }
        public string PropertyName => Property.Name;
        private bool CollectionItemsAreTrackable { get; set; }

        public object OldValue
        {
            get { return _oldValue; }
            set
            {
                _oldValue = value;

                if (_oldValue != null && _oldValue.CanBeTrackedAsCollection())
                {
                    IEnumerable enumerable = _oldValue as IEnumerable;

                    if (enumerable != null)
                    {
                        _oldCollectionValue = enumerable.CloneEnumerable();

                        if (_oldValue is IProxyTargetAccessor)
                            _oldCollectionValue = (IEnumerable)TrackableObjectFactory.CreateForCollection
                            (
                                _oldCollectionValue, (IChangeTrackableObject)TargetObject, Property
                            );

                        _oldValue = _oldCollectionValue;

                        CollectionItemsAreTrackable = TrackerDogConfiguration.CanTrackType(_oldValue.GetCollectionItemType());
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
                    return CurrentValue != OldValue;
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

                            if (trackable != null) return trackable.ChangeTracker.ChangedProperties.Count > 0;
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