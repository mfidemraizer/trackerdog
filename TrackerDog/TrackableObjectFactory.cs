namespace TrackerDog
{
    using Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using TrackerDog.Configuration;
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
            Contract.Requires(parentObject != null, "A parent object to the given collection is mandatory");
            Contract.Requires(parentObjectProperty != null, "A non-null reference to the property holding the collection is mandatory");
            Contract.Ensures(Contract.Result<object>() != null);

            if (some == null)
                some = TrackerDogConfiguration.CollectionConfiguration
                                .GetImplementation(parentObjectProperty.PropertyType).Value
                                .CreateInstanceWithGenericArgs(null, parentObjectProperty.PropertyType.GetGenericArguments()[0]);

            Contract.Assert(some != null, "Either if a collection object is provided or not, a proxied instance of the whole collection type must be created");

            Type genericCollectionType = some.GetType().GetGenericArguments()[0];

            if (TrackerDogConfiguration.CanTrackType(genericCollectionType))
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
        /// <param name="typeToTrack">The type to track</param>
        /// <param name="reusedTracker">An optional object change tracker to reuse instead of creating a new one</param>
        /// <param name="parentObject">An optional parent object if object to be proxied is associated to other object</param>
        /// <param name="propertyToSet">An optional parent object property of the given parent object where given object to be proxied is held in the other side of the association</param>
        /// <returns>The change-trackable proxy of the given object</returns>
        public static object Create(object some = null, Type typeToTrack = null, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null, object[] constructorArguments = null)
        {
            Contract.Requires(some != null || typeToTrack != null, "Either an object or type to track must be provided");

            typeToTrack = typeToTrack ?? some.GetType();

            if (TrackerDogConfiguration.CanTrackType(typeToTrack) && !typeToTrack.IsTrackable())
            {
                ProxyGenerationOptions options = new ProxyGenerationOptions(new SimplePropertyInterceptionHook());
                options.AddMixinInstance(new ChangeTrackableObjectMixin());

                List<IInterceptor> interceptors = new List<IInterceptor>
                {
                    new SimplePropertyInterceptor()
                };

                if (typeToTrack.IsDynamicObject())
                    interceptors.Add(new DynamicObjectInterceptor());

                object proxy;

                if (some != null)
                    proxy = ProxyGenerator.CreateClassProxyWithTarget
                    (
                        classToProxy: typeToTrack,
                        additionalInterfacesToProxy: new[] { typeof(IChangeTrackableObject) },
                        target: some,
                        options: options,
                        interceptors: interceptors.ToArray()
                    );
                else
                    proxy = ProxyGenerator.CreateClassProxy
                    (
                        classToProxy: typeToTrack,
                        additionalInterfacesToProxy: new[] { typeof(IChangeTrackableObject) },
                        options: options,
                        constructorArguments: constructorArguments,
                        interceptors: interceptors.ToArray()
                    );


                IChangeTrackableObject trackableObject = (IChangeTrackableObject)proxy;
                trackableObject.StartTracking(trackableObject, reusedTracker);

                IEnumerable<PropertyInfo> propertiesToTrack =
                    TrackerDogConfiguration.GetTrackableType(typeToTrack).IncludedProperties;

                if (propertiesToTrack.Count() == 0)
                    propertiesToTrack = typeToTrack.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo property in propertiesToTrack)
                {
                    if (!property.IsIndexer())
                    {
                        object propertyValue = property.GetValue(trackableObject);

                        if (propertyValue != null)
                            Create(propertyValue, trackableObject.ChangeTracker, proxy, property);
                        else
                            Create(property.PropertyType, trackableObject.ChangeTracker, proxy, property);
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
        public static TObject Create<TObject>(TObject some = null, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null, object[] constructorArguments = null)
            where TObject : class
        {
            return (TObject)Create(some, some == null ? typeof(TObject) : null, reusedTracker, parentObject, propertyToSet, constructorArguments);
        }
    }
}