namespace TrackerDog
{
    using Castle.DynamicProxy;
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using TrackerDog.Hooks;
    using TrackerDog.Interceptors;
    using TrackerDog.Mixins;

    /// <summary>
    /// Represents a factory of change-trackable objects.
    /// </summary>
    internal static class TrackableObjectFactory
    {
        private readonly static ProxyGenerator _proxyGenerator = new ProxyGenerator();

        /// <summary>
        /// Gets current proxy generator
        /// </summary>
        private static ProxyGenerator ProxyGenerator => _proxyGenerator;

        /// <summary>
        /// Creates a change-trackable proxy of a collection instance. It also turns into change-trackable objects all 
        /// associated object associations in the object graph.
        /// </summary>
        /// <param name="some">The collection instance</param>
        /// <param name="parentObject">The other side of the 1-n association</param>
        /// <param name="parentObjectProperty">The property in the other side of the 1-n association</param>
        /// <returns>The change-trackable proxy of the given collection object</returns>
        public static object CreateForCollection(object some, IChangeTrackableObject parentObject, PropertyInfo parentObjectProperty)
        {
            Contract.Requires(parentObject != null);
            Contract.Requires(parentObjectProperty != null);

            if (some == null)
                some = TrackerDogConfiguration.CollectionConfiguration
                                .GetImplementation(parentObjectProperty.PropertyType).Value
                                .CreateInstanceWithGenericArgs(null, parentObjectProperty.PropertyType.GetGenericArguments()[0]);

            Contract.Assert(some != null);

            Type genericCollectionType = some.GetType().GetGenericArguments()[0];

            if (TrackerDogConfiguration.TrackableTypes.Contains(genericCollectionType))
            {
                ProxyGenerationOptions options = new ProxyGenerationOptions(new CollectionterceptionHook());
                options.AddMixinInstance(new ChangeTrackableCollectionMixin());

                KeyValuePair<Type, Type> collectionImplementationDetail = TrackerDogConfiguration.CollectionConfiguration.GetImplementation(some.GetType());

                object targetList =
                    collectionImplementationDetail.Value.CreateInstanceWithGenericArgs
                    (
                        new[]
                        {
                            typeof(EnumerableExtensions).GetMethod("MakeAllTrackable")
                                            .MakeGenericMethod(genericCollectionType)
                                            .Invoke(null, new[] { some, parentObjectProperty, parentObject })
                        },
                        genericCollectionType
                    );

                IChangeTrackableCollection proxy = (IChangeTrackableCollection)ProxyGenerator.CreateInterfaceProxyWithTarget
                (
                    collectionImplementationDetail.Key.MakeGenericType(genericCollectionType),
                    new[] { typeof(IChangeTrackableCollection), typeof(IReadOnlyChangeTrackableCollection) },
                    targetList,
                    options,
                    new CollectionPropertyInterceptor()
                );

                proxy.ParentObject = parentObject;
                proxy.ParentObjectProperty = parentObjectProperty;

                proxy.CollectionChanged += (sender, e) =>
                    parentObject.RaisePropertyChanged(parentObject, parentObjectProperty.Name);

                return proxy;
            }
            else return some;
        }

        /// <summary>
        /// Creates a change-trackable proxy of the given object. It also turns into change-trackable objects all 
        /// associated object associations in the object graph.
        /// </summary>
        /// <param name="some">The object to be trackable</param>
        /// <param name="reusedTracker">An optional object change tracker to reuse instead of creating a new one</param>
        /// <param name="parentObject">An optional parent object if object to be proxied is associated to other object</param>
        /// <param name="propertyToSet">An optional parent object property of the given parent object where given object to be proxied is held in the other side of the association</param>
        /// <returns>The change-trackable proxy of the given object</returns>
        public static object Create(object some, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null)
        {
            Contract.Requires(some != null);

            if (TrackerDogConfiguration.TrackableTypes.Contains(some.GetType()) && !(some is IChangeTrackableObject))
            {
                ProxyGenerationOptions options = new ProxyGenerationOptions(new SimplePropertyInterceptionHook());
                options.AddMixinInstance(new ChangeTrackableObjectMixin());

                object proxy = ProxyGenerator.CreateClassProxyWithTarget
                (
                    some.GetType(),
                    new[] { typeof(IChangeTrackableObject) },
                    some,
                    options,
                    new SimplePropertyInterceptor()
                );

                IChangeTrackableObject trackableObject = (IChangeTrackableObject)proxy;
                trackableObject.StartTracking(trackableObject, reusedTracker);

                foreach (PropertyInfo property in
                    trackableObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.IsIndexer())
                    {
                        object propertyValue = property.GetValue(trackableObject);

                        if (propertyValue != null)
                            Create(propertyValue, trackableObject.ChangeTracker, proxy, property);
                    }
                }

                if (propertyToSet != null)
                    propertyToSet.SetValue(parentObject ?? proxy, proxy);

                trackableObject.AcceptChanges();

                return proxy;
            }
            else return some;
        }

        /// <summary>
        /// Creates a change-trackable proxy of the given object. It also turns into change-trackable objects all 
        /// associated object associations in the object graph.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to be tracked</typeparam>
        /// <param name="some">The object to be trackable</param>
        /// <param name="reusedTracker">An optional object change tracker to reuse instead of creating a new one</param>
        /// <param name="parentObject">An optional parent object if object to be proxied is associated to other object</param>
        /// <param name="propertyToSet">An optional parent object property of the given parent object where given object to be proxied is held in the other side of the association</param>
        /// <returns>The change-trackable proxy of the given object</returns>
        public static TObject Create<TObject>(TObject some, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null)
        {
            return (TObject)Create((object)some, reusedTracker, parentObject, propertyToSet);
        }
    }
}