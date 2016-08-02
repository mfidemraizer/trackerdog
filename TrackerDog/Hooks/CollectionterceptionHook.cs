using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Hooks
{
    internal sealed class CollectionterceptionHook : IProxyGenerationHook
    {
        private readonly static Guid _id = Guid.NewGuid();
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private static readonly HashSet<MethodInfo> _skippedMethods;

        static CollectionterceptionHook()
        {
            _skippedMethods = new HashSet<MethodInfo>
            (
                typeof(IHasParent).GetMethods(DefaultBindingFlags)
                .Concat(typeof(IChangeTrackableCollection).GetMethods(DefaultBindingFlags))
                .Concat(typeof(INotifyCollectionChanged).GetMethods(DefaultBindingFlags))
            );
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
            => !SkippedMethods.Contains(methodInfo) && !methodInfo.IsPropertyGetter();

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            CollectionterceptionHook mixin = obj as CollectionterceptionHook;

            if (mixin == null) return false;

            return mixin.Id == Id;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}