using Castle.DynamicProxy;
using TrackerDog.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Hooks
{
    internal sealed class SimplePropertyInterceptionHook : IProxyGenerationHook
    {
        private readonly static Guid _id = Guid.NewGuid();
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private static readonly HashSet<MethodInfo> _skippedMethods;
        private static readonly HashSet<MethodInfo> _dynamicObjectGetterSetterMethods;

        public SimplePropertyInterceptionHook(IObjectChangeTrackingConfiguration configuration)
        {
            Configuration = configuration;
        }

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

        public IObjectChangeTrackingConfiguration Configuration { get; }

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
            bool isDynamicObject = type.IsDynamicObject();

            if ((!isDynamicObject && !methodInfo.IsPropertyGetterOrSetter()) || _skippedMethods.Contains(methodInfo))
                return false;

            ITrackableType trackableType = Configuration.TrackableTypes.SingleOrDefault(t => t.Type == type);

            if (trackableType == null)
                return false;

            if (isDynamicObject && methodInfo.IsMethodOfDynamicObject())
                return _dynamicObjectGetterSetterMethods.Contains(methodInfo.GetRuntimeBaseDefinition());

            if (trackableType.IncludedProperties.Count == 0)
                return true;

            return trackableType.IncludedProperties
                                .Concat
                                (
                                    Configuration.GetAllTrackableBaseTypes(trackableType)
                                                            .SelectMany(t => t.IncludedProperties)
                                ).Any(p =>
                                {
                                    var accessorMethods = new[] { p.GetMethod.GetBaseDefinition(), p.SetMethod.GetBaseDefinition() };

                                    return accessorMethods.Contains(methodInfo.GetBaseDefinition())
                                            || accessorMethods.Any(m => m.DeclaringType.IsAssignableFrom(methodInfo.GetBaseDefinition().DeclaringType) && m.Name == methodInfo.Name);
                                });
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