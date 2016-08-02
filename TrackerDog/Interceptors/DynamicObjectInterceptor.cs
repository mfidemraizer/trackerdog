using Castle.DynamicProxy;
using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Reflection;
using TrackerDog.Configuration;

namespace TrackerDog.Interceptors
{
    internal sealed class DynamicObjectInterceptor : MethodInterceptor
    {
        private readonly static Guid _id = Guid.NewGuid();

        public DynamicObjectInterceptor(IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactory trackableObjectFactory)
        {
            Configuration = configuration;
            TrackableObjectFactory = trackableObjectFactory;
        }

        private IObjectChangeTrackingConfiguration Configuration { get; }
        private ITrackableObjectFactory TrackableObjectFactory { get; }

        internal Guid Id => _id;

        protected override void InterceptMethod(IInvocation invocation)
        {
            invocation.Proceed();

            if (invocation.Proxy is DynamicObject && invocation.Method.GetRuntimeBaseDefinition().DeclaringType == typeof(DynamicObject))
            {
                SetMemberBinder setBinder = invocation.Arguments[0] as SetMemberBinder;

                if (setBinder != null)
                {
                    IChangeTrackableObject trackableObject = invocation.Proxy as IChangeTrackableObject;
                    Contract.Assert(trackableObject != null);

                    if (!invocation.Arguments[1].IsTrackable() && Configuration.CanTrackType(invocation.Arguments[1].GetType()))
                    {
                        object trackableArgument = TrackableObjectFactory.CreateFrom(invocation.Arguments[1]);

                        if (trackableObject is IChangeTrackableObject)
                            trackableObject.SetDynamicMember(setBinder.Name, trackableArgument);
                    }

                    trackableObject.RaisePropertyChanged(trackableObject, setBinder.Name);
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            DynamicObjectInterceptor interceptor = obj as DynamicObjectInterceptor;

            if (interceptor == null) return false;

            return interceptor.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}