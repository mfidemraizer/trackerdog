namespace TrackerDog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
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
        /// the values are <see cref="ObjectPropertyChangeTracking"/>.
        /// </summary>
        private Dictionary<PropertyInfo, ObjectPropertyChangeTracking> PropertyTrackings { get; }
            = new Dictionary<PropertyInfo, ObjectPropertyChangeTracking>();

        public IImmutableSet<IObjectPropertyChangeTracking> ChangedProperties =>
            PropertyTrackings.Where(t => t.Value.HasChanged)
                        .Select(t => (IObjectPropertyChangeTracking)t.Value)
                        .ToImmutableHashSet();
        public IImmutableSet<IObjectPropertyChangeTracking> UnchangedProperties =>
            PropertyTrackings.Where(t => !t.Value.HasChanged)
                        .Select(t => (IObjectPropertyChangeTracking)t.Value)
                        .ToImmutableHashSet();

        public void Complete()
        {
            Contract.Assert(PropertyTrackings != null);

            if (PropertyTrackings.Count > 0)
                foreach (KeyValuePair<PropertyInfo, ObjectPropertyChangeTracking> tracking in PropertyTrackings)
                {
                    Contract.Assert(ReferenceEquals(tracking.Value.Tracker, this));

                    tracking.Value.OldValue = tracking.Value.CurrentValue;
                }

        }

        public void Discard()
        {
            Contract.Assert(PropertyTrackings != null);

            if (PropertyTrackings.Count > 0)
                foreach (KeyValuePair<PropertyInfo, ObjectPropertyChangeTracking> tracking in PropertyTrackings)
                {
                    Contract.Assert(ReferenceEquals(tracking.Value.Tracker, this));

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
            Contract.Requires(property != null);
            Contract.Requires(targetObject == null || property.DeclaringType.IsAssignableFrom(targetObject.GetType()));
            targetObject = targetObject ?? TargetObject;

            object currentValue = targetObject.GetType().GetProperty(property.Name, DefaultBindingFlags)
                                            .GetValue(targetObject);

            ObjectPropertyChangeTracking existingTracking;

            Contract.Assert(PropertyTrackings != null);

            if (!PropertyTrackings.TryGetValue(property, out existingTracking))
                PropertyTrackings.Add
                (
                    property,
                    new ObjectPropertyChangeTracking(this, targetObject, property, currentValue)
                );
            else
                existingTracking.CurrentValue = currentValue;
        }

        public IEnumerator<IObjectPropertyChangeTracking> GetEnumerator() => PropertyTrackings.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            MemberExpression memberExpr = propertySelector.Body as MemberExpression;
            Contract.Assert(memberExpr != null);

            PropertyInfo property = memberExpr.Member as PropertyInfo;
            Contract.Assert(property != null);
            Contract.Assert(PropertyTrackings != null);

            return PropertyTrackings.Single(t => t.Key.DeclaringType.BaseType.GetProperty(t.Key.Name) == property)
                            .Value;
        }
    }
}