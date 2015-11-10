namespace TrackerDog.Patterns
{
    /// <summary>
    /// Defines what should provide a basic unit of work to accept or discard object changes.
    /// </summary>
    public interface IObjectChangeUnitOfWork
    {
        void Complete();
        void Discard();
    }
}