using System;
using System.Linq.Expressions;

namespace TrackerDog.Contracts
{
    public static class Contract
    {
        public static void Requires(Expression<Func<bool>> conditionExpression, string message = null)
        {
            Func<bool> conditionFunc = conditionExpression.Compile();

            if (!conditionFunc())
                throw new ContractException("Pre-condition failed: " + (string.IsNullOrEmpty(message) ? $"{conditionExpression.ToString()}" : $"{conditionExpression.ToString()} -> {message}"));
        }

        public static void Assert(Expression<Func<bool>> conditionExpression, string message = null)
        {
            Func<bool> conditionFunc = conditionExpression.Compile();

            if (!conditionFunc())
                throw new ContractException("Assertion failed: " + (string.IsNullOrEmpty(message) ? $"{conditionExpression.ToString()}" : $"{conditionExpression.ToString()} -> {message}"));
        }
    }
}