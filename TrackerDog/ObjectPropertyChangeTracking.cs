namespace TrackerDog
{
    using Castle.DynamicProxy;
    using System.Collections;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay("{Property.DeclaringType.Name}.{Property.Name} = {CurrentValue} (Has changed? {HasChanged})")]
    internal sealed class ObjectPropertyChangeTracking : IObjectPropertyChangeTracking
    {
        private readonly ObjectChangeTracker _tracker;
        private readonly object _targetObject;
        private object _oldValue;
        private object _currentValue;
        private IEnumerable _oldCollectionValue;
        private IEnumerable _currentCollectionValue;

        public ObjectPropertyChangeTracking(ObjectChangeTracker tracker, object targetObject, PropertyInfo property, object currentValue)
        {
            _tracker = tracker;
            _targetObject = targetObject;
            Property = property;
            OldValue = currentValue;
            CurrentValue = currentValue;
        }

        public ObjectChangeTracker Tracker => _tracker;
        IObjectChangeTracker IObjectPropertyChangeTracking.Tracker => Tracker;
        public object TargetObject => _targetObject;
        public PropertyInfo Property { get; private set; }

        public object OldValue
        {
            get { return _oldValue; }
            set
            {
                _oldValue = value;

                if (!(_oldValue is string))
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

        public bool HasChanged => !IsCollection ?
            CurrentValue != OldValue :
                (
                    OldCollectionValue.Cast<object>().Count() != CurrentCollectionValue.Cast<object>().Count()
                    ||
                    CurrentCollectionValue.Cast<object>().Any(o =>
                    {
                        IChangeTrackableObject trackable = o as IChangeTrackableObject;

                        if (trackable != null) return trackable.ChangeTracker.ChangedProperties.Count > 0;
                        else return false;
                    })
                );

        public override bool Equals(object obj) => Equals(obj as IObjectPropertyChangeTracking);

        public bool Equals(IObjectPropertyChangeTracking other) =>
            other != null && other.Property == Property;

        public override int GetHashCode() =>
            Property.Name.GetHashCode() + Property.DeclaringType.AssemblyQualifiedName.GetHashCode();
    }
}