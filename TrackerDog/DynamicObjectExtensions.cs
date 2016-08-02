using FastMember;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace TrackerDog
{
    /// <summary>
    /// Represents a set of <see cref="System.Dynamic.DynamicObject"/> extension methods.
    /// </summary>
    internal static class DynamicObjectExtensions
    {
        private static readonly Dictionary<Type, TypeAccessor> _typeAccessors = new Dictionary<Type, TypeAccessor>();
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

            TypeAccessor accessor;

            if (!_typeAccessors.TryGetValue(obj.GetType(), out accessor))
                _typeAccessors.Add(obj.GetType(), (accessor = TypeAccessor.Create(obj.GetType())));

            return accessor[obj, memberName];

        }
        /// <summary>
        /// Calls a dynamic property to set its value
        /// </summary>
        /// <param name="obj">The dynamic object</param>
        /// <param name="memberName">The property name to gets its value</param>
        /// <param name="value">The property value to set</param>
        /// <returns>The property value</returns>
        public static void SetDynamicMember(this object obj, string memberName, object value)
        {
            Contract.Requires(obj != null, "The dynamic object must be not null");
            Contract.Requires(!string.IsNullOrEmpty(memberName), "The member name must be provided");

            TypeAccessor accessor;

            if (!_typeAccessors.TryGetValue(obj.GetType(), out accessor))
                _typeAccessors.Add(obj.GetType(), (accessor = TypeAccessor.Create(obj.GetType())));

            accessor[obj, memberName] = value;
        }
    }
}