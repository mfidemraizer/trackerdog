namespace TrackerDog
{
    public interface ITrackableObjectFactory
    {
        /// <summary>
        /// Creates a completely new trackable object of the given type
        /// </summary>
        /// <typeparam name="TObject">The type of the object to track</typeparam>
        /// <param name="args">Constructor arguments in order, if the whole type has a constructor with arguments.</param>
        /// <returns>The change-trackable object</returns>
        TObject CreateOf<TObject>(params object[] args)
               where TObject : class;

        /// <summary>
        /// Creates a new trackable object wrapping a non-trackable one.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to track</typeparam>
        /// <param name="some">The whole object to wrap as trackable object</param>
        /// <returns>The change-trackable object</returns>
        TObject CreateFrom<TObject>(TObject some)
            where TObject : class;
    }
}
