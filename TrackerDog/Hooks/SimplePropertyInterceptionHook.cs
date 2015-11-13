namespace TrackerDog.Hooks
{
    using Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using TrackerDog.Configuration;

    internal sealed class SimplePropertyInterceptionHook : IProxyGenerationHook
    {
        private readonly static Guid _id = Guid.NewGuid();
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private static readonly HashSet<MethodInfo> _skippedMethods;
        private static readonly HashSet<MethodInfo> _dynamicObjectGetterSetterMethods;

        static SimplePropertyInterceptionHook()
        {
            _skippedMethods = new HashSet<MethodInfo>
            (
                typeof(IHasParent).GetMethods(DefaultBindingFlags)
                .Concat(typeof(IChangeTrackableObject).GetMethods(DefaultBindingFlags))
            );

            _dynamicObjectGetterSetterMethods = new HashSet<MethodInfo>
            {
                typeof(DynamicObject).GetMethod("TryGetMember"),
                typeof(DynamicObject).GetMethod("TrySetMember")
            };
        }

        internal Guid Id => _id;
        private static HashSet<MethodInfo> SkippedMethods => _skippedMethods;

        public void MethodsInspected()
        {
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            if (_skippedMethods.Contains(methodInfo))
                return false;

            ITrackableType trackableType = TrackerDogConfiguration.TrackableTypes.SingleOrDefault(t => t.Type == type);

            if (trackableType == null)
                return false;

            if (type.IsDynamicObject())
                return _dynamicObjectGetterSetterMethods.Contains(methodInfo.GetRuntimeBaseDefinition());

            if (trackableType.IncludedProperties.Count == 0)
                return true;

            return trackableType.IncludedProperties.Any(p => p.GetMethod == methodInfo || p.SetMethod == methodInfo);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            SimplePropertyInterceptionHook mixin = obj as SimplePropertyInterceptionHook;

            if (mixin == null) return false;

            return mixin.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}