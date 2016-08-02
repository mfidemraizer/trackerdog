using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TrackerDog.Configuration;
using TrackerDog.Patterns;

namespace TrackerDog
{
    /// <summary>
    /// Represents an in-memory object property change tracker.
    /// </summary>
    [DebuggerDisplay("Changed properties: {ChangedProperties.Count}")]
    internal class ObjectChangeTracker : IObjectChangeUnitOfWork, IObjectChangeTracker, IEnumerable<IObjectPropertyChangeTracking>
    {
        private readonly Guid _id = Guid.NewGuid();
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        private readonly object _targetObject;
        private readonly Type _targetObjectType;
        private readonly object _syncLock = new object();

        private event EventHandler<ObjectChangeEventArgs> _Changed;

        public event EventHandler<ObjectChangeEventArgs> Changed
        {
            add { _Changed += value; }
            remove { _Changed -= value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="targetObject">The object to track its changes</param>
        public ObjectChangeTracker(IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactoryInternal trackableObjectFactory, object targetObject)
        {
            Contract.Requires(targetObject != null);

            Configuration = configuration;
            TrackableObjectFactory = trackableObjectFactory;
            _targetObject = targetObject;
            _targetObjectType = targetObject.GetType();
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }
        private ITrackableObjectFactoryInternal TrackableObjectFactory { get; }

        /// <summary>
        /// Gets the object to track its changes by current tracker
        /// </summary>
        private object TargetObject => _targetObject;

        /// <summary>
        /// Gets the object type of the o bject to track its changed by current tracker
        /// </summary>
        private Type TargetObjectType => _targetObjectType;

        /// <summary>
        /// Gets a dictionary of tracked property states, where the keys are instances of <see cref="System.Reflection.PropertyInfo"/> and
        /// the values are <see cref="DeclaredObjectPropertyChangeTracking"/>.
        /// </summary>
        internal Dictionary<PropertyInfo, DeclaredObjectPropertyChangeTracking> PropertyTrackings { get; }
            = new Dictionary<PropertyInfo, DeclaredObjectPropertyChangeTracking>();

        internal Dictionary<string, ObjectPropertyChangeTracking> DynamicPropertyTrackings { get; }
            = new Dictionary<string, ObjectPropertyChangeTracking>();

        public IImmutableSet<IObjectPropertyChangeTracking> ChangedProperties =>
            PropertyTrackings.Where(t => t.Value.HasChanged).Select(t => (IObjectPropertyChangeTracking)t.Value)
                        .Concat(DynamicPropertyTrackings.Where(t => t.Value.HasChanged).Select(t => (IObjectPropertyChangeTracking)t.Value))
                        .ToImmutableHashSet();

        public IImmutableSet<IObjectPropertyChangeTracking> UnchangedProperties =>
            PropertyTrackings.Where(t => !t.Value.HasChanged).Select(t => (IObjectPropertyChangeTracking)t.Value)
                        .Concat(DynamicPropertyTrackings.Where(t => !t.Value.HasChanged).Select(t => (IObjectPropertyChangeTracking)t.Value))
                        .ToImmutableHashSet();

        public void Complete()
        {
            lock (_syncLock)
            {
                Contract.Assert(PropertyTrackings != null, "Property trackings cannot be null to accept all changes");

                if (PropertyTrackings.Count > 0)
                    foreach (KeyValuePair<PropertyInfo, DeclaredObjectPropertyChangeTracking> tracking in PropertyTrackings)
                    {
                        Contract.Assert(ReferenceEquals(tracking.Value.Tracker, this), "This tracking must be tracked by current change tracker");

                        tracking.Value.OldValue = tracking.Value.CurrentValue;
                    }
            }
        }

        public void Discard()
        {
            lock (_syncLock)
            {
                Contract.Assert(PropertyTrackings != null, "Property trackings cannot be null to undo all changes");

                if (PropertyTrackings.Count > 0)
                    foreach (KeyValuePair<PropertyInfo, DeclaredObjectPropertyChangeTracking> tracking in PropertyTrackings)
                    {
                        Contract.Assert(ReferenceEquals(tracking.Value.Tracker, this), "This tracking must be tracked by current change tracker");

                        tracking.Value.CurrentValue = tracking.Value.OldValue;
                        tracking.Value.Property.SetValue(tracking.Value.TargetObject, tracking.Value.OldValue);
                    }
            }
        }

        public IObjectGraphTrackingInfo GetTrackingGraphFromProperty(PropertyInfo property)
        {
            DeclaredObjectPropertyChangeTracking propertyTracking = null;
            PropertyTrackings.TryGetValue(property.GetBaseProperty(), out propertyTracking);

            List<IDeclaredObjectPropertyChangeTracking> hierarchy = new List<IDeclaredObjectPropertyChangeTracking>();

            bool searchTracking = true;

            while (searchTracking)
            {
                hierarchy.Add(propertyTracking);

                var result = PropertyTrackings.SingleOrDefault
                (
                    t => t.Value.CurrentValue == propertyTracking.TargetObject
                        && t.Value.Property.PropertyType.GetProperty(propertyTracking.Property.Name) == propertyTracking.Property.GetBaseProperty()
                );

                if (result.Value != null)
                {
                    propertyTracking = result.Value;
                    property = propertyTracking.Property;
                }
                else
                    searchTracking = false;
            }

            Contract.Assert(propertyTracking != null);

            return new ObjectGraphTrackingInfo
            (
                propertyTracking.TargetObject, 
                hierarchy.Reverse<IDeclaredObjectPropertyChangeTracking>().ToImmutableList()
            );
        }

        /// <summary>
        /// Adds or updates a tracked property state.
        /// </summary>
        /// <param name="property">The tracked property</param>
        /// <param name="targetObject">The object owning the tracked property</param>
        public void AddOrUpdateTracking(PropertyInfo property, object targetObject = null)
        {
            Contract.Requires(property != null, "Since this is tracking a property, the property itself cannot be null");
            Contract.Requires(targetObject == null || property.DeclaringType.IsAssignableFrom(targetObject.GetType()), "Given property must be declared on the given target object");

            lock (_syncLock)
            {
                targetObject = targetObject ?? TargetObject;

                object currentValue = targetObject.GetType().GetProperty(property.Name, DefaultBindingFlags)
                                                .GetValue(targetObject);

                DeclaredObjectPropertyChangeTracking existingTracking = null;

                Contract.Assert(PropertyTrackings != null, "Cannot add or update a tracking if tracking collection is null");

                PropertyInfo baseProperty = property.GetBaseProperty();

                DeclaredObjectPropertyChangeTracking tracking = null;

                if (Configuration.CanTrackProperty(property) && !PropertyTrackings.TryGetValue(baseProperty, out existingTracking))
                {
                    DeclaredObjectPropertyChangeTracking declaredTracking = new DeclaredObjectPropertyChangeTracking(Configuration, TrackableObjectFactory, this, targetObject, baseProperty, property, currentValue);
                    tracking = declaredTracking;

                    PropertyTrackings.Add(baseProperty, declaredTracking);
                }
                else if (existingTracking != null)
                {
                    existingTracking.CurrentValue = currentValue;
                    tracking = existingTracking;
                }

                if (tracking != null && tracking.HasChanged)
                    _Changed?.Invoke(this, new DeclaredObjectPropertyChangeEventArgs(targetObject, tracking));
            }
        }

        /// <summary>
        /// Adds or updates a tracked property state.
        /// </summary>
        /// <param name="propertyName">The tracked property name</param>
        /// <param name="targetObject">The object owning the tracked property</param>
        public void AddOrUpdateTracking(string propertyName, DynamicObject targetObject = null)
        {
            Contract.Requires(propertyName != null, "Since this is tracking a property, the property property itself cannot be null or empty");

            lock (_syncLock)
            {
                targetObject = targetObject ?? TargetObject as DynamicObject;

                Contract.Assert(targetObject != null, "To add or update a tracking by the property name, the object owning the property must be a dynamic object");

                object currentValue = targetObject.GetDynamicMember(propertyName);

                ObjectPropertyChangeTracking existingTracking = null;

                Contract.Assert(DynamicPropertyTrackings != null, "Cannot add a property tracking if tracking collection is null");

                IObjectPropertyChangeTracking tracking = null;

                if (!DynamicPropertyTrackings.TryGetValue(propertyName, out existingTracking))
                {
                    ObjectPropertyChangeTracking propertyTracking = new ObjectPropertyChangeTracking(Configuration, TrackableObjectFactory, this, targetObject, propertyName, currentValue);
                    DynamicPropertyTrackings.Add(propertyName, propertyTracking);
                    tracking = propertyTracking;
                }
                else if (existingTracking != null)
                {
                    existingTracking.CurrentValue = currentValue;
                    tracking = existingTracking;
                }

                if (tracking != null && tracking.HasChanged)
                    _Changed?.Invoke(this, new ObjectChangeEventArgs(targetObject, tracking));
            }
        }

        public IEnumerator<IObjectPropertyChangeTracking> GetEnumerator() => PropertyTrackings.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IDeclaredObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            return GetTrackingByProperty(propertySelector.ExtractProperty());
        }

        public IObjectPropertyChangeTracking GetDynamicTrackingByProperty(string propertyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(propertyName), "Property name cannot be null or empty");

            lock (_syncLock)
            {
                return DynamicPropertyTrackings[propertyName];
            }
        }

        public IDeclaredObjectPropertyChangeTracking GetTrackingByProperty(PropertyInfo property)
        {
            Contract.Assert(property != null, "Selected member is not a property");
            Contract.Assert(PropertyTrackings != null, "Cannot get the property tracking if tracking collection is null");

            lock (_syncLock)
            {
                return PropertyTrackings.Single(t => t.Key.DeclaringType.GetActualTypeIfTrackable().GetProperty(t.Key.Name) == property)
                                .Value;
            }
        }
    }
}