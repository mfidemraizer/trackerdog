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
                ),
                "An object which declares that has a parent object must provide a non-null reference to the whole parent object"
            );

            Contract.Invariant
            (
                ParentObjectProperty != null
                && ParentObjectProperty.DeclaringType == ParentObject.GetType(),
                "An object which declares that has a parent object must provide a non-null reference to the property which holds the parent object"
            );
        }
    }
}
