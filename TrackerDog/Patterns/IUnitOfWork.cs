namespace TrackerDog.Patterns
{
    public interface IUnitOfWork
    {
        void Complete();
        void Discard();
    }
}