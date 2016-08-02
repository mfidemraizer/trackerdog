namespace TrackerDog
{
    public interface ITrackableObjectFactory
    {
        TObject CreateOf<TObject>(params object[] args)
               where TObject : class;

        TObject CreateFrom<TObject>(TObject some)
            where TObject : class;
    }
}
