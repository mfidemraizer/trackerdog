namespace TrackerDog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
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
            where TObject : class
        {
            Contract.Requires(some != null, "Reference must not be null to turn an object into a trackable one");

            return TrackableObjectFactory.Create(some: some);
        }

        /// <summary>
        /// Turns some object into a trackable object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to track changes</typeparam>
        /// <param name="some">The object to track its changes</param>
        /// <param name="propertyToSet">The property to which the proxy must be set to</param>
        /// <returns>A proxy of the given object to track its changes</returns>
        internal static TObject AsTrackable<TObject>(this TObject some, PropertyInfo propertyToSet)
            where TObject : class
        {
            Contract.Requires(some != null, "Reference must not be null to turn an object into a trackable one");

            return TrackableObjectFactory.Create(some: some, propertyToSet: propertyToSet);
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

        public static Type GetActualTypeIfTrackable(this object some)
        {
            Contract.Requires(some != null, "Reference must not be null to get actual object type");
            Contract.Ensures(Contract.Result<Type>() != null);

            return GetActualTypeIfTrackable(some.GetType());
        }

        public static Type GetActualTypeIfTrackable(this Type some)
        {
            Contract.Requires(some != null, "Reference must not be null to get actual object type");
            Contract.Ensures(Contract.Result<Type>() != null);

            if (some.IsTrackable())
                return some.BaseType;
            else
                return some;
        }

        /// <summary>
        /// Determines if a given type is already change-trackable
        /// </summary>
        /// <param name="some">The type to check</param>
        /// <returns><literal>true</literal> if it's change-trackable, <literal>false</literal> if it's not change-trackable</returns>
        public static bool IsTrackable(this Type some)
        {
            Contract.Requires(some != null, "Reference must not be null to check if a type is trackable");

            return typeof(IChangeTrackableObject).IsAssignableFrom(some);
        }

        /// <summary>
        /// Gets current tracked object change tracker.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        /// <returns>The change tracker</returns>
        public static IObjectChangeTracker GetChangeTracker(this object some)
        {
            Contract.Requires(some != null, "Reference must not be null to get object change tracker");
            Contract.Ensures(Contract.Result<IObjectChangeTracker>() != null);

            IChangeTrackableObject trackableObject = some as IChangeTrackableObject;

            if (trackableObject == null)
            {
                IHasParent withParent = some as IHasParent;

                if (withParent != null)
                    trackableObject = withParent.ParentObject;
            }

            Contract.Assert(trackableObject != null, "An object must be trackable in order to get its change tracker");

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
        public static IDeclaredObjectPropertyChangeTracking GetPropertyTracking<TObject, TReturn>(this TObject some, Expression<Func<TObject, TReturn>> propertySelector)
        {
            Contract.Requires(some != null, "Reference must not be null because the given object is required to get its change tracker and find the desired property tracking");
            Contract.Requires(propertySelector != null, "A property selector is mandatory to get a property tracking");

            return some.GetChangeTracker().GetTrackingByProperty(propertySelector) as IDeclaredObjectPropertyChangeTracking;
        }

        /// <summary>
        /// Gets a property change tracking for a given property
        /// </summary>
        /// <param name="some">The tracked object</param>
        /// <param name="propertyName">A property selector</param>
        /// <returns>The property tracking</returns>
        public static IObjectPropertyChangeTracking GetPropertyTracking(this object some, string propertyName)
        {
            Contract.Requires(some != null, "Reference must not be null because the given object is required to get its change tracker and find the desired property tracking");
            Contract.Requires(!string.IsNullOrEmpty(propertyName), "A property name is mandatory to get a property tracking");

            ObjectChangeTracker tracker = (ObjectChangeTracker)some.GetChangeTracker();

            if (tracker.DynamicPropertyTrackings.ContainsKey(propertyName))
                return tracker.GetDynamicTrackingByProperty(propertyName);
            else
            {
                Dictionary<string, DeclaredObjectPropertyChangeTracking> declaredPropertyTrackings = tracker.PropertyTrackings
                                                                    .ToDictionary(t => t.Key.Name, t => t.Value);

                DeclaredObjectPropertyChangeTracking declaredPropertyTracking;

                if (declaredPropertyTrackings.TryGetValue(propertyName, out declaredPropertyTracking))
                    return declaredPropertyTracking;
                else
                    throw new InvalidOperationException
                    (
                        $"Cannot locate a tracking for the given property name '{propertyName}'. This can be either caused because there is more than a property with the given name or actually the given property is not being tracked for changes"
                    );
            }
        }

        /// <summary>
        /// Accepts all changes made to the change-tracked object and its associations.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        public static void AcceptChanges(this object some)
        {
            Contract.Requires(some != null, "A non-null reference is required to accept tracked object changes");

            IChangeTrackableObject trackableObject = some as IChangeTrackableObject;

            Contract.Assert(trackableObject != null, "Given object must be trackable to accept its changes");

            trackableObject.ChangeTracker.Complete();
        }

        /// <summary>
        /// Undoes all changes made to the change-tracked object and its associations.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        public static void UndoChanges(this object some)
        {
            Contract.Requires(some != null, "A non-null reference is required to undo tracked object changes");

            IChangeTrackableObject trackableObject = some as IChangeTrackableObject;

            Contract.Assert(trackableObject != null, "Given object must be trackable to undo its changes");

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
            Contract.Requires(some != null, "A non-null reference is mandatory to get an object old value property");
            Contract.Requires(some is IChangeTrackableObject, "Given object must be trackable");
            Contract.Ensures(Contract.Result<TReturn>() != null);

            return (TReturn)some.GetPropertyTracking(propertySelector).OldValue;
        }

        /// <summary>
        /// Gets the value of given selected property that had when the change-tracked object started to track its changes.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>The value of the property when it was started to be tracked</returns>
        /// </example>
        public static dynamic OldPropertyValue(this object some, string propertyName)
        {
            Contract.Requires(some != null, "A non-null reference is mandatory to get an object old property value");
            Contract.Requires(some is IChangeTrackableObject, "Given object must be trackable");

            return some.GetPropertyTracking(propertyName).OldValue;
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
            Contract.Requires(some != null, "A non-null reference is mandatory to get an object current property value");
            Contract.Requires(some is IChangeTrackableObject, "Given object must be trackable");
            Contract.Ensures(Contract.Result<TReturn>() != null);

            return (TReturn)some.GetPropertyTracking(propertySelector).CurrentValue;
        }

        /// <summary>
        /// Gets the last value of given selected property.
        /// </summary>
        /// <param name="some">The change-tracked object</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>The last value of the property</returns>
        public static dynamic CurrentPropertyValue(this object some, string propertyName)
        {
            Contract.Requires(some != null, "A non-null reference is mandatory to get an object current property value");
            Contract.Requires(some is IChangeTrackableObject, "Given object must be trackable");

            return some.GetPropertyTracking(propertyName).CurrentValue;
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
            Contract.Requires(some != null, "A non-null reference is mandatory to check if a property has changed");
            Contract.Requires(some is IChangeTrackableObject, "Given object must be trackable");

            return some.GetPropertyTracking(propertySelector).HasChanged;
        }

        /// <summary>
        /// Determines if a given property by name has changed since its tracking was started.
        /// </summary>
        /// <param name="some">The trackable object</param>
        /// <param name="propertyName">The property selector</param>
        /// <returns><codeInline>true</codeInline> if it has changed, <codeInline>false</codeInline> if it doesn't changed.</returns>
        public static bool PropertyHasChanged(this object some, string propertyName)
        {
            Contract.Requires(some != null, "A non-null reference is mandatory to check if a property has changed");
            Contract.Requires(some is IChangeTrackableObject, "Given object must be trackable");

            return some.GetPropertyTracking(propertyName).HasChanged;
        }
    }
}