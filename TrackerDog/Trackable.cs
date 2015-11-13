namespace TrackerDog
{
    /// <summary>
    /// Represents a façade over <see cref="TrackableObjectFactory"/> to allow creating trackable objects
    /// from types instead of turning instances into trackable objects.
    /// </summary>
    public static class Trackable
    {
        /// <summary>
        /// Creates a trackable object of given type and optionally takes type's constructor arguments.
        /// </summary>
        /// <typeparam name="TObject">The type to create a trackable instance</typeparam>
        /// <param name="args">Possible constructor arguments in order</param>
        /// <returns>The trackable object</returns>
        public static TObject Of<TObject>(params object[] args)
            where TObject : class => TrackableObjectFactory.Create<TObject>(constructorArguments: args);
    }
}