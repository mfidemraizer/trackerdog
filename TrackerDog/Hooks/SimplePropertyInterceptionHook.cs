namespace TrackerDog.Hooks
{
    using Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;

    internal sealed class SimplePropertyInterceptionHook : IProxyGenerationHook
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private static readonly HashSet<MethodInfo> _skippedMethods;

        static SimplePropertyInterceptionHook()
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
        {
            ITrackableType trackableType = TrackerDogConfiguration.TrackableTypes.SingleOrDefault(t => t.Type == type);

            if (trackableType == null || trackableType.IncludedProperties.Count == 0)
                return false;

            return trackableType.IncludedProperties.Any(p => p.GetMethod == methodInfo || p.SetMethod == methodInfo);
        }
    }
}