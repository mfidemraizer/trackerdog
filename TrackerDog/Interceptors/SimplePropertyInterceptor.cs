namespace TrackerDog.Interceptors
{
    using Castle.DynamicProxy;
    using System;
    using System.Reflection;

    internal sealed class SimplePropertyInterceptor : PropertyInterceptor
    {
        private readonly static Guid _id = Guid.NewGuid();
        internal Guid Id => _id;

        protected override void InterceptGetter(IInvocation invocation, PropertyInfo property, IChangeTrackableObject trackableObject)
        {
        }

        protected override void InterceptSetter(IInvocation invocation, PropertyInfo property, IChangeTrackableObject trackableObject)
        {
            trackableObject.RaisePropertyChanged(trackableObject, property.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            SimplePropertyInterceptor interceptor = obj as SimplePropertyInterceptor;

            if (interceptor == null) return false;

            return interceptor.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}