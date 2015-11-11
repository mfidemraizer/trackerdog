namespace TrackerDog.Configuration
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a factory of trackable type configuration.
    /// </summary>
    public static class Track
    {
        /// <summary>
        /// Creates an instance of <see cref="TrackableType{T}"/> to fluently configure it.
        /// </summary>
        /// <typeparam name="T">The type to track</typeparam>
        /// <returns>A <see cref="TrackableType{T}"/> instance</returns>
        public static TrackableType<T> ThisType<T>()
        {
            Contract.Ensures(Contract.Result<TrackableType<T>>() != null);

            return new TrackableType<T> { Type = typeof(T) };
        }
    }
}