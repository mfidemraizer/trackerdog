namespace TrackerDog
{
    using System.Diagnostics.Contracts;
    using System.Reflection;

    [ContractClassFor(typeof(IHasParent))]
    internal abstract class IHasParentContract : IHasParent
    {
        public IChangeTrackableObject ParentObject { get; set; }
        public PropertyInfo ParentObjectProperty { get; set; }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant
            (
                ParentObject != null
                &&
                (
                    ParentObjectProperty == null
                    ||
                    (
                        ParentObjectProperty != null
                        && ParentObjectProperty.DeclaringType == ParentObject.GetType()
                    )
                )
            );

            Contract.Invariant
            (
                ParentObjectProperty != null
                && ParentObjectProperty.DeclaringType == ParentObject.GetType()
            );
        }
    }
}
