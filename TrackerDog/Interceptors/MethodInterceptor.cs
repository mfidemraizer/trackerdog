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

            IHasParent withParent = invocation.Proxy as IHasParent;

            if (withParent != null && invocation.Method.IsPublic && !invocation.Method.IsPropertyGetterOrSetter())
                InterceptMethod(invocation, withParent);
            else
                InterceptMethod(invocation);
        }

        protected virtual void InterceptMethod(IInvocation invocation, IHasParent withParent)
        {
        }

        protected virtual void InterceptMethod(IInvocation invocation)
        {
        }
    }
}