namespace TrackerDog.Interceptors
{
    using Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.Specialized;
    using System.Linq;
    using CollectionHandling;
    using TrackerDog.Configuration;

    internal sealed class CollectionPropertyInterceptor : MethodInterceptor
    {
        private readonly static Guid _id = Guid.NewGuid();
        internal Guid Id => _id;

        protected override void InterceptMethod(IInvocation invocation, IHasParent withParent)
        {
            Type collectionType = invocation.Method.DeclaringType;

            if ((collectionType.IsList() || collectionType.IsSet()) && invocation.Arguments.Length == 1)
            {
                IChangeTrackableCollection trackableCollection = (IChangeTrackableCollection)withParent;
                IChangeTrackableObject changedItem =
                    invocation.Arguments[0] as IChangeTrackableObject
                    ?? invocation.Arguments[0].AsTrackable() as IChangeTrackableObject;
                IEnumerable<object> currentItems = ((IEnumerable<object>)withParent).ToList();

                invocation.Arguments[0] = changedItem ?? invocation.Arguments[0];
                invocation.Proceed();

                KeyValuePair<Type, CollectionImplementation> implementation =
                    TrackerDogConfiguration.CollectionConfiguration.GetImplementation(collectionType);

                CollectionChange whatChange = implementation.Value.TrackingHandler.HandleTracking
                (
                    new CollectionChangeContext
                    {
                        Collection = (IEnumerable<object>)trackableCollection,
                        ItemsBefore = currentItems,
                        ParentObjectProperty = trackableCollection.ParentObjectProperty,
                        CalledMember = invocation.Method,
                        CallArguments = invocation.Arguments.ToImmutableList()
                    },
                    trackableCollection.AddedItems,
                    trackableCollection.RemovedItems
                );

                switch (whatChange)
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
