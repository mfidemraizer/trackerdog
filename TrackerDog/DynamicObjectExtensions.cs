namespace TrackerDog
{
    using Microsoft.CSharp.RuntimeBinder;
    using System;
    using System.Diagnostics.Contracts;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a set of <see cref="System.Dynamic.DynamicObject"/> extension methods.
    /// </summary>
    internal static class DynamicObjectExtensions
    {
        /// <summary>
        /// Calls a dynamic property to get its value
        /// </summary>
        /// <param name="obj">The dynamic object</param>
        /// <param name="memberName">The property name to gets its value</param>
        /// <returns>The property value</returns>
        public static object GetDynamicMember(this object obj, string memberName)
        {
            Contract.Requires(obj != null, "The dynamic object must be not null");
            Contract.Requires(!string.IsNullOrEmpty(memberName), "The member name must be provided");

            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);

            return callsite.Target(callsite, obj);
        }
    }
}