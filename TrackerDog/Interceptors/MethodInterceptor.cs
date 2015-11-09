namespace TrackerDog.Interceptors
{
    using Castle.DynamicProxy;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal abstract class MethodInterceptor : IInterceptor
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance;

        public void Intercept(IInvocation invocation)
        {
            Contract.Assume(invocation != null);

            if (invocation.Method.IsPublic && !invocation.Method.IsPropertyGetterOrSetter())
            {
                InterceptMethod(invocation, (IHasParent)invocation.Proxy);
            }
        }

        protected abstract void InterceptMethod(IInvocation invocation, IHasParent withParent);
    }
}