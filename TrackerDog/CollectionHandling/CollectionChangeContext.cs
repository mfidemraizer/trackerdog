namespace TrackerDog.CollectionHandling
{
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class CollectionChangeContext
    {
        public IEnumerable<object> Collection { get; internal set; }
        public IEnumerable<object> ItemsBefore { get; internal set; }
        public PropertyInfo ParentObjectProperty { get; internal set; }
        public MemberInfo CalledMember { get; internal set; }
        public IEnumerable<object> CallArguments { get; internal set; }
    }
}