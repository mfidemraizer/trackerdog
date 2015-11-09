namespace TrackerDog.Interceptors
{
    using Castle.DynamicProxy;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal sealed class SimplePropertyInterceptor : PropertyInterceptor
    {
        protected override void InterceptGetter(IInvocation invocation, PropertyInfo property, IChangeTrackableObject trackableObject)
        {
        }

        protected override void InterceptSetter(IInvocation invocation, PropertyInfo property, IChangeTrackableObject trackableObject)
        {
            trackableObject.RaisePropertyChanged(trackableObject, property.Name);
        }
    }
}