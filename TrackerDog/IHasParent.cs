namespace TrackerDog
{
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Defines an object that has a 1-1 association with some parent object that is already change-trackable
    /// </summary>
    [ContractClass(typeof(IHasParentContract))]
    internal interface IHasParent
    {
        /// <summary>
        /// Gets the parent change-trackable object
        /// </summary>
        IChangeTrackableObject ParentObject { get; set; }

        /// <summary>
        /// Gets the parent change-trackable object property holding current side of the 1-1 association
        /// </summary>
        PropertyInfo ParentObjectProperty { get; set; }
    }
}