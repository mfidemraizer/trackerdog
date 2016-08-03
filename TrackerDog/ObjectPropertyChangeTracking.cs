using Castle.DynamicProxy;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using TrackerDog.Configuration;

namespace TrackerDog
{
    [DebuggerDisplay("{PropertyName} = {CurrentValue} (Has changed? {HasChanged})")]
    internal sealed class ObjectPropertyChangeTracking : IObjectPropertyChangeTracking
    {
        private readonly ObjectChangeTracker _tracker;
        private readonly object _targetObject;
        private object _oldValue;
        private object _currentValue;
        private IEnumerable _oldCollectionValue;
        private IEnumerable _currentCollectionValue;

        public ObjectPropertyChangeTracking(IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactoryInternal trackableObjectFactory, ObjectChangeTracker tracker, object targetObject, string propertyName, object currentValue)
        {
            Configuration = configuration;
            TrackableObjectFactory = trackableObjectFactory;
            _tracker = tracker;
            _targetObject = targetObject;
            PropertyName = propertyName;
            OldValue = currentValue;
            CurrentValue = currentValue;
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }
        private ITrackableObjectFactoryInternal TrackableObjectFactory { get; }

        public ObjectChangeTracker Tracker => _tracker;
        IObjectChangeTracker IObjectPropertyChangeTracking.Tracker => Tracker;
        public object TargetObject => _targetObject;
        public string PropertyName { get; private set; }

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
                        _oldCollectionValue = enumerable.CloneEnumerable(Configuration);

                        if (_oldValue is IProxyTargetAccessor)
                            _oldCollectionValue = (IEnumerable)TrackableObjectFactory.CreateForCollection
                            (
                                _oldCollectionValue, (IChangeTrackableObject)TargetObject, null
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

                        if (trackable != null) return trackable.GetChangeTrackingContext().ChangeTracker.ChangedProperties.Count > 0;
                        else return false;
                    })
                );

        public override bool Equals(object obj) => Equals(obj as IDeclaredObjectPropertyChangeTracking);

        public bool Equals(IObjectPropertyChangeTracking other) =>
            other != null && other.PropertyName == PropertyName;

        public override int GetHashCode() => PropertyName.GetHashCode();

    }
}