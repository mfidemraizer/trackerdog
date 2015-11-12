namespace TrackerDog
{
    using Microsoft.CSharp.RuntimeBinder;
    using System;
    using System.Dynamic;
    using System.Runtime.CompilerServices;

    internal static class DynamicObjectExtensions
    {
        public static object GetDynamicMember(this object obj, string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);

            return callsite.Target(callsite, obj);
        }
    }
}