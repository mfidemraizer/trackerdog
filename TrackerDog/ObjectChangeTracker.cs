namespace TrackerDog
{
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

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="targetObject">The object to track its changes</param>
        public ObjectChangeTracker(object targetObject)
        {
            Contract.Requires(targetObject != null);
            _targetObject = targetObject;
            _targetObjectType = targetObject.GetType();
        }

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
            Contract.Assert(PropertyTrackings != null, "Property trackings cannot be null to accept all changes");

            if (PropertyTrackings.Count > 0)
                foreach (KeyValuePair<PropertyInfo, DeclaredObjectPropertyChangeTracking> tracking in PropertyTrackings)
                {
                    Contract.Assert(ReferenceEquals(tracking.Value.Tracker, this), "This tracking must be tracked by current change tracker");

                    tracking.Value.OldValue = tracking.Value.CurrentValue;
                }

        }

        public void Discard()
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

        /// <summary>
        /// Adds or updates a tracked property state.
        /// </summary>
        /// <param name="property">The tracked property</param>
        /// <param name="targetObject">The object owning the tracked property</param>
        public void AddOrUpdateTracking(PropertyInfo property, object targetObject = null)
        {
            Contract.Requires(property != null, "Since this is tracking a property, the property itself cannot be null");
            Contract.Requires(targetObject == null || property.DeclaringType.IsAssignableFrom(targetObject.GetType()), "Given property must be declared on the given target object");
            targetObject = targetObject ?? TargetObject;

            object currentValue = targetObject.GetType().GetProperty(property.Name, DefaultBindingFlags)
                                            .GetValue(targetObject);

            DeclaredObjectPropertyChangeTracking existingTracking = null;

            Contract.Assert(PropertyTrackings != null, "Cannot add or update a tracking if tracking collection is null");

            PropertyInfo baseProperty = property.GetBaseProperty();

            if (TrackerDogConfiguration.CanTrackProperty(property) && !PropertyTrackings.TryGetValue(baseProperty, out existingTracking))
                PropertyTrackings.Add
                (
                    baseProperty,
                    new DeclaredObjectPropertyChangeTracking(this, targetObject, property, currentValue)
                );
            else if (existingTracking != null)
                existingTracking.CurrentValue = currentValue;
        }

        /// <summary>
        /// Adds or updates a tracked property state.
        /// </summary>
        /// <param name="propertyName">The tracked property name</param>
        /// <param name="targetObject">The object owning the tracked property</param>
        public void AddOrUpdateTracking(string propertyName, DynamicObject targetObject = null)
        {
            Contract.Requires(propertyName != null, "Since this is tracking a property, the property property itself cannot be null or empty");

            targetObject = targetObject ?? TargetObject as DynamicObject;

            Contract.Assert(targetObject != null, "To add or update a tracking by the property name, the object owning the property must be a dynamic object");

            object currentValue = targetObject.GetDynamicMember(propertyName);

            ObjectPropertyChangeTracking existingTracking = null;

            Contract.Assert(DynamicPropertyTrackings != null, "Cannot add a property tracking if tracking collection is null");

            if (!DynamicPropertyTrackings.TryGetValue(propertyName, out existingTracking))
                DynamicPropertyTrackings.Add
                (
                    propertyName,
                    new ObjectPropertyChangeTracking(this, targetObject, propertyName, currentValue)
                );
            else if (existingTracking != null)
                existingTracking.CurrentValue = currentValue;
        }

        public IEnumerator<IObjectPropertyChangeTracking> GetEnumerator() => PropertyTrackings.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IDeclaredObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            PropertyInfo property = propertySelector.ExtractProperty();

            Contract.Assert(property != null, "Selected member is not a property");
            Contract.Assert(PropertyTrackings != null, "Cannot get the property tracking if tracking collection is null");

            return PropertyTrackings.Single(t => t.Key.DeclaringType.GetActualTypeIfTrackable().GetProperty(t.Key.Name) == property)
                            .Value;
        }

        public IObjectPropertyChangeTracking GetDynamicTrackingByProperty(string propertyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(propertyName), "Property name cannot be null or empty");

            return DynamicPropertyTrackings[propertyName];
        }
    }
}