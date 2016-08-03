using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using TrackerDog.Configuration;
using TrackerDog.Hooks;
using TrackerDog.Interceptors;
using TrackerDog.Mixins;

namespace TrackerDog
{
    internal sealed class TrackableObjectFactoryInternal : ITrackableObjectFactoryInternal, ITrackableObjectFactory
    {
        public TrackableObjectFactoryInternal(IObjectChangeTrackingConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }

        /// <summary>
        /// Gets current proxy generator
        /// </summary>
        private ProxyGenerator ProxyGenerator { get; } = new ProxyGenerator();

        public object Create(object some = null, Type typeToTrack = null, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null, object[] constructorArguments = null)
        {
            typeToTrack = typeToTrack ?? some.GetType();

            ITrackableType interfaceTrackableType = null;

            if ((Configuration.CanTrackType(typeToTrack) || Configuration.ImplementsBaseType(typeToTrack, out interfaceTrackableType)) && !typeToTrack.IsTrackable())
            {
                if (interfaceTrackableType != null)
                {
                    Configuration.TrackThisTypeRecursive
                    (
                        typeToTrack,
                        trackableType =>
                        {
                            if (trackableType.Type == typeToTrack)
                                ((ICanConfigureTrackableType<ITrackableType>)trackableType).IncludeProperties
                                (
                                    interfaceTrackableType.IncludedProperties.ToArray()
                                );
                        }
                    );
                }

                Contract.Assert(typeToTrack.IsClass && !typeToTrack.IsAbstract && !typeToTrack.IsSealed, "The object type to track must be a non-abstract, non-sealed class");

                ProxyGenerationOptions options = new ProxyGenerationOptions(new SimplePropertyInterceptionHook(this.Configuration));
                options.AddMixinInstance(new ChangeTrackableObjectMixin(Configuration, this));
                options.AdditionalAttributes.Add(AttributeUtil.CreateBuilder(typeof(DebuggerDisplayAttribute), new[] { $"{typeToTrack.FullName}Proxy" }));

                List<IInterceptor> interceptors = new List<IInterceptor>
                {
                    new SimplePropertyInterceptor()
                };

                if (typeToTrack.IsDynamicObject())
                    interceptors.Add(new DynamicObjectInterceptor(Configuration, this));

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

                HashSet<PropertyInfo> propertiesToTrack =
                    new HashSet<PropertyInfo>(Configuration.GetTrackableType(typeToTrack).IncludedProperties);

                if (propertiesToTrack.Count() == 0)
                    foreach (PropertyInfo property in typeToTrack.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        propertiesToTrack.Add(property);

                foreach (PropertyInfo property in propertiesToTrack)
                {
                    if (!property.IsIndexer())
                    {
                        object propertyValue = property.GetValue(trackableObject);

                        if (propertyValue != null)
                            Create(propertyValue, trackableObject.GetChangeTrackingContext().ChangeTracker, proxy, property);
                        else
                            Create(property.PropertyType, trackableObject.GetChangeTrackingContext().ChangeTracker, proxy, property);
                    }
                }

                if (propertyToSet != null)
                    propertyToSet.SetValue(parentObject ?? proxy, proxy);

                trackableObject.AcceptChanges();

                return proxy;
            }
            else return some;
        }

        public TObject Create<TObject>(TObject some = null, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null, object[] constructorArguments = null)
            where TObject : class
        {
            return (TObject)Create(some, some == null ? typeof(TObject) : null, reusedTracker, parentObject, propertyToSet, constructorArguments);
        }

        public object CreateForCollection(object some, IChangeTrackableObject parentObject, PropertyInfo parentObjectProperty)
        {
            if (some.IsTrackable())
                return some;

            Type collectionType = Configuration.Collections
                            .GetImplementation(parentObjectProperty.PropertyType).Key;

            Type collectionImplementation;

            if (parentObjectProperty.PropertyType.IsGenericType && parentObjectProperty.PropertyType == collectionType.MakeGenericType(parentObjectProperty.PropertyType.GenericTypeArguments))
                collectionImplementation = collectionType.MakeGenericType(parentObjectProperty.PropertyType.GenericTypeArguments);
            else
                collectionImplementation = parentObjectProperty.PropertyType.GetInterfaces().SingleOrDefault
                (
                    i =>
                    {
                        return i.IsGenericType && i == collectionType.MakeGenericType(i.GenericTypeArguments[0]);
                    }
                );

            Contract.Assert(collectionImplementation != null);

            if (some == null)
                some = Configuration.Collections
                                .GetImplementation(parentObjectProperty.PropertyType).Value.Type
                                .CreateInstanceWithGenericArgs(null, collectionImplementation.GenericTypeArguments[0]);

            Contract.Assert(some != null, "Either if a collection object is provided or not, a proxied instance of the whole collection type must be created");

            Type genericCollectionType = some.GetType().GetGenericArguments().Last();
            bool canTrackCollectionType = Configuration.CanTrackType(genericCollectionType);

            ProxyGenerationOptions options = new ProxyGenerationOptions(new CollectionterceptionHook());
            options.AddMixinInstance(new ChangeTrackableCollectionMixin());

            KeyValuePair<Type, CollectionImplementation> collectionImplementationDetail
                        = Configuration.Collections.GetImplementation(parentObjectProperty.PropertyType);

            Contract.Assert(collectionImplementationDetail.Key.MakeGenericType(collectionImplementation.GenericTypeArguments).IsAssignableFrom(parentObjectProperty.PropertyType), $"Trackable collection implementation of type '{collectionImplementationDetail.Key.AssemblyQualifiedName}' cannot be set to the target property '{parentObjectProperty.DeclaringType.FullName}.{parentObjectProperty.Name}' with type '{parentObjectProperty.PropertyType.AssemblyQualifiedName}'. This isn't supported because it might require a downcast. Please provide a collection change tracking configuration to work with the more concrete interface.");

            object targetList;

            if (!canTrackCollectionType)
                targetList = some;
            else
                targetList = collectionImplementationDetail.Value.Type.CreateInstanceWithGenericArgs
                (
                    new[]
                    {
                            typeof(EnumerableExtensions).GetMethod("MakeAllTrackable")
                                            .MakeGenericMethod(genericCollectionType)
                                            .Invoke(null, new[] { some, Configuration, this, parentObjectProperty, parentObject })
                    },
                    genericCollectionType
                );

            Contract.Assert(targetList != null, "List to proxy is mandatory");

            IChangeTrackableCollection proxy = (IChangeTrackableCollection)ProxyGenerator.CreateInterfaceProxyWithTarget
            (
                collectionImplementationDetail.Key.MakeGenericType(some.GetType().GetGenericArguments()),
                new[] { typeof(IChangeTrackableCollection), typeof(IReadOnlyChangeTrackableCollection) },
                targetList,
                options,
                new CollectionPropertyInterceptor(Configuration, this)
            );

            proxy.ParentObject = parentObject;
            proxy.ParentObjectProperty = parentObjectProperty;

            proxy.CollectionChanged += (sender, e) =>
                parentObject.RaisePropertyChanged(parentObject, parentObjectProperty.Name);

            return proxy;
        }

        public TObject CreateFrom<TObject>(TObject some) where TObject : class => Create(some: some);

        public TObject CreateOf<TObject>(params object[] args) where TObject : class => Create<TObject>(constructorArguments: args);
    }
}