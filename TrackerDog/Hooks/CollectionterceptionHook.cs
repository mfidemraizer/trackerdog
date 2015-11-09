namespace TrackerDog.Hooks
{
    using Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal sealed class CollectionterceptionHook : IProxyGenerationHook
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private static readonly HashSet<MethodInfo> _skippedMethods;

        static CollectionterceptionHook()
        {
            _skippedMethods = new HashSet<MethodInfo>
            (
                typeof(IHasParent).GetMethods(DefaultBindingFlags)
                .Concat(typeof(IChangeTrackableObject).GetMethods(DefaultBindingFlags))
            );
        }

        private static HashSet<MethodInfo> SkippedMethods => _skippedMethods;

        public void MethodsInspected()
        {
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            => !SkippedMethods.Contains(methodInfo) && !methodInfo.IsPropertyGetter();
    }
}