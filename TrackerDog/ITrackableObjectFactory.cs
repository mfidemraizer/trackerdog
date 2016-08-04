namespace TrackerDog
{
    public interface ITrackableObjectFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        TObject CreateOf<TObject>(params object[] args)
               where TObject : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="some"></param>
        /// <returns></returns>
        TObject CreateFrom<TObject>(TObject some)
            where TObject : class;
    }
}
