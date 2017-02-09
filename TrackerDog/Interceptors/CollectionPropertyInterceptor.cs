using Castle.DynamicProxy;
using TrackerDog.CollectionHandling;
using TrackerDog.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Interceptors
{
    internal sealed class CollectionPropertyInterceptor : MethodInterceptor
    {
        private readonly static Guid _id = Guid.NewGuid();

        public CollectionPropertyInterceptor(IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactory trackableObjectFactory)
        {
            Configuration = configuration;
            TrackableObjectFactory = trackableObjectFactory;
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }
        private ITrackableObjectFactory TrackableObjectFactory { get; }

        internal Guid Id => _id;

        protected override void InterceptMethod(IInvocation invocation, IHasParent withParent)
        {
            Type collectionType = invocation.Method.DeclaringType;

            if (collectionType.IsEnumerable() && invocation.Arguments.Length == 1)
            {
                IChangeTrackableCollection trackableCollection = (IChangeTrackableCollection)withParent;
                IChangeTrackableObject changedItem =
                    invocation.Arguments[0] as IChangeTrackableObject
                    ?? TrackableObjectFactory.CreateFrom(invocation.Arguments[0]) as IChangeTrackableObject;

                IEnumerable<object> currentItems;
                Type collectionItemType = withParent.GetCollectionItemType();
                bool itemsAreKeyValuePair = collectionItemType.GetTypeInfo().IsGenericType && collectionItemType == typeof(KeyValuePair<,>).MakeGenericType(collectionItemType.GenericTypeArguments);

                if (itemsAreKeyValuePair)
                    currentItems = ((IEnumerable)withParent).Cast<object>();
                else
                    currentItems = ((IEnumerable<object>)withParent).ToList();

                invocation.Arguments[0] = changedItem ?? invocation.Arguments[0];
                invocation.Proceed();

                KeyValuePair<Type, CollectionImplementation> implementation =
                    Configuration.Collections.GetImplementation(collectionType);

                CollectionChangeContext changeContext = new CollectionChangeContext
                (
                    !itemsAreKeyValuePair ? (IEnumerable<object>)trackableCollection : Enumerable.Empty<object>(),
                    currentItems,
                    trackableCollection.ParentObjectProperty,
                    trackableCollection.AddedItems,
                    trackableCollection.RemovedItems
                );

                if (!itemsAreKeyValuePair)
                    Activator.CreateInstance(implementation.Value.ChangeInterceptor.MakeGenericType(trackableCollection.GetCollectionItemType()), changeContext)
                                .CallMethod(invocation.Method.Name, invocation.Arguments);

                switch (changeContext.Change)
                {
                    case CollectionChange.Add:
                        trackableCollection.RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new[] { changedItem });
                        break;

                    case CollectionChange.Remove:
                        trackableCollection.RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { changedItem });
                        break;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            CollectionPropertyInterceptor interceptor = obj as CollectionPropertyInterceptor;

            if (interceptor == null) return false;

            return interceptor.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
