using System;

namespace TrackerDog.Contracts
{
    public class ContractException : Exception
    {
        public ContractException(string message) 
            : base(message)
        {
        }
    }
}