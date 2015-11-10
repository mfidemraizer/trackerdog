namespace TrackerDog.Interceptors
{
    using Castle.DynamicProxy;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal abstract class PropertyInterceptor : IInterceptor
    {
        private readonly static HashSet<string> _changeTrackingMembers;
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        
        static PropertyInterceptor()
        {
            _changeTrackingMembers =
                new HashSet<string>(typeof(IChangeTrackableObject).GetMembers().Select(m => m.Name));
        }

        public static HashSet<string> ChangeTrackingMembers => _changeTrackingMembers;

        public void Intercept(IInvocation invocation)
        {
            Contract.Assume(invocation != null);

            invocation.Proceed();

            if (invocation.Method.IsPublic)
            {
                string propertName = invocation.Method.NormalizePropertyGetterSetterName();

                if (!ChangeTrackingMembers.Contains(propertName))
                {
                    IChangeTrackableObject trackableObject = invocation.Proxy as IChangeTrackableObject;

                    Contract.Assert(trackableObject != null);

                    if (invocation.Method.IsPropertyGetter())
                        InterceptGetter
                        (
                            invocation,
                            invocation.InvocationTarget.GetType().GetProperty(propertName, DefaultBindingFlags),
                            trackableObject
                        );
                    else if (invocation.Method.IsPropertySetter())
                        InterceptSetter
                        (
                            invocation,
                            invocation.InvocationTarget.GetType().GetProperty(propertName, DefaultBindingFlags),
                            trackableObject
                        );
                }
            }
        }

        protected abstract void InterceptGetter(IInvocation invocation, PropertyInfo property, IChangeTrackableObject trackableObject);
        protected abstract void InterceptSetter(IInvocation invocation, PropertyInfo property, IChangeTrackableObject trackableObject);
    }
}