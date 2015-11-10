namespace TrackerDog
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Represents a set of object change-tracking related operations that work as façades to simplify the work
    /// with change tracking.
    /// </summary>
    public static class ObjectChangeTrackingExtensions
    {
        /// <summary>
        /// Turns some object into a trackable object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to track changes</typeparam>
        /// <param name="some">The object to track its changes</param>
        /// <returns>A proxy of the given object to track its changes</returns>
        public static TObject AsTrackable<TObject>(this TObject some)
        {
            Contract.Requires(some != null);

            return TrackableObjectFactory.Create(some);
        }

        /// <summary>
        /// Turns some object into a trackable object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to track changes</typeparam>
        /// <param name="some">The object to track its changes</param>
        /// <param name="propertyToSet">The property to which the proxy must be set to</param>
        /// <returns>A proxy of the given object to track its changes</returns>
        internal static TObject AsTrackable<TObject>(this TObject some, PropertyInfo propertyToSet)
        {
            Contract.Requires(some != null);

            return TrackableObjectFactory.Create(some, propertyToSet: propertyToSet);
        }

        /// <summary>
        /// Determines if a given object is a change-trackable object already
        /// </summary>
        /// <param name="some">The object to check</param>
        /// <returns><literal>true</literal> if it's change-trackable, <literal>false</literal> if it's not change-trackable</returns>
        public static bool IsTrackable(this object some)
        {
            return some is IChangeTrackableObject;
        }

        /// <summary>
        /// Gets current tracked object change tracker.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        /// <returns>The change tracker</returns>
        public static IObjectChangeTracker GetChangeTracker(this object some)
        {
            Contract.Requires(some != null);
            Contract.Ensures(Contract.Result<IObjectChangeTracker>() != null);

            IChangeTrackableObject trackableObject = some as IChangeTrackableObject;

            if (trackableObject == null)
            {
                IHasParent withParent = some as IHasParent;

                if (withParent != null)
                    trackableObject = withParent.ParentObject;
            }

            Contract.Assert(trackableObject != null);

            return trackableObject.ChangeTracker;
        }

        /// <summary>
        /// Gets a property change tracking for a given property
        /// </summary>
        /// <typeparam name="TObject">The type of tracked object</typeparam>
        /// <typeparam name="TReturn">The type of the property</typeparam>
        /// <param name="some">The tracked object</param>
        /// <param name="propertySelector">A property selector</param>
        /// <returns>The property tracking</returns>
        public static IObjectPropertyChangeTracking GetPropertyTracking<TObject, TReturn>(this TObject some, Expression<Func<TObject, TReturn>> propertySelector)
        {
            Contract.Requires(some != null);
            Contract.Requires(propertySelector != null);

            return some.GetChangeTracker().GetTrackingByProperty(propertySelector);
        }

        /// <summary>
        /// Accepts all changes made to the change-tracked object and its associations.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        public static void AcceptChanges(this object some)
        {
            Contract.Requires(some != null);

            IChangeTrackableObject trackableObject = some as IChangeTrackableObject;

            Contract.Assert(trackableObject != null);

            trackableObject.ChangeTracker.Complete();
        }

        /// <summary>
        /// Undoes all changes made to the change-tracked object and its associations.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        public static void UndoChanges(this object some)
        {
            Contract.Requires(some != null);

            IChangeTrackableObject trackableObject = some as IChangeTrackableObject;

            Contract.Assert(trackableObject != null);

            trackableObject.ChangeTracker.Discard();
        }

        /// <summary>
        /// Gets the value of given selected property that had when the change-tracked object started to track its changes.
        /// </summary>
        /// <typeparam name="T">The type of the change-tracked object</typeparam>
        /// <typeparam name="TReturn">The type of the property to gets its unchanged value</typeparam>
        /// <param name="some">The change-tracked object</param>
        /// <param name="propertySelector">The property selector</param>
        /// <returns>The value of the property when it was started to be tracked</returns>
        /// <example>
        /// <code language="c#">
        /// var oldValue = some.OldPropertyValue(o => o.Text);
        /// </code>
        /// </example>
        public static TReturn OldPropertyValue<T, TReturn>(this T some, Expression<Func<T, TReturn>> propertySelector)
        {
            Contract.Requires(some != null);
            Contract.Requires(some is IChangeTrackableObject);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<TReturn>() != null);

            return (TReturn)some.GetPropertyTracking(propertySelector).OldValue;
        }

        /// <summary>
        /// Gets the last value of given selected property.
        /// </summary>
        /// <typeparam name="T">The type of the change-tracked object</typeparam>
        /// <typeparam name="TReturn">The type of the property to gets its last value</typeparam>
        /// <param name="some">The change-tracked object</param>
        /// <param name="propertySelector">The property selector</param>
        /// <returns>The last value of the property</returns>
        /// <example>
        /// <code language="c#">
        /// var currentValue = some.CurrentPropertyValue(o => o.Text);
        /// </code>
        /// </example>
        public static TReturn CurrentPropertyValue<T, TReturn>(this T some, Expression<Func<T, TReturn>> propertySelector)
        {
            Contract.Requires(some != null);
            Contract.Requires(some is IChangeTrackableObject);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<TReturn>() != null);

            return (TReturn)some.GetPropertyTracking(propertySelector).CurrentValue;
        }

        /// <summary>
        /// Determines if a given property by selector has changed since its tracking was started.
        /// </summary>
        /// <typeparam name="T">The type of the object owning the whole property</typeparam>
        /// <param name="some">The change-tracked object</param>
        /// <param name="propertySelector">The property selector</param>
        /// <returns><codeInline>true</codeInline> if it has changed, <codeInline>false</codeInline> if it doesn't changed.</returns>
        public static bool PropertyHasChanged<T>(this T some, Expression<Func<T, object>> propertySelector)
        {

            Contract.Requires(some != null);
            Contract.Requires(some is IChangeTrackableObject);
            Contract.Requires(propertySelector != null);

            return some.GetPropertyTracking(propertySelector).HasChanged;
        }
    }
}