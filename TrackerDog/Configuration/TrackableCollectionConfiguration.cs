namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public sealed class TrackableCollectionConfiguration
    {
        public TrackableCollectionConfiguration()
        {
            AddImplementation(typeof(ISet<>), typeof(HashSet<>));
            AddImplementation(typeof(IList<>), typeof(List<>));
            AddImplementation(typeof(ICollection<>), typeof(List<>));
        }

        internal Dictionary<Type, Type> Implementations { get; } = new Dictionary<Type, Type>();

        public KeyValuePair<Type, Type> GetImplementation(Type some)
        {
            KeyValuePair<Type, Type> result = Implementations
                .FirstOrDefault
                (
                    interfaceType => 
                        some.GetInterfaces().Any
                        (
                            i => i.IsGenericType 
                                && i.GetGenericTypeDefinition() == interfaceType.Key
                        )
                );

            if (result.Key != null)
                return result;
            else
                throw new InvalidOperationException($"No implementation found to '{some.AssemblyQualifiedName}'");
        }

        public void AddImplementation(Type interfaceType, Type implementationType)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(interfaceType.IsInterface);
            Contract.Requires(implementationType != null);
            Contract.Requires(implementationType.IsClass && !implementationType.IsAbstract);

            Implementations.Add(interfaceType, implementationType);
        }

        public void ReplaceImplementation(Type interfaceType, Type implementationType)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(interfaceType.IsInterface);
            Contract.Requires(implementationType != null);
            Contract.Requires(implementationType.IsClass && !implementationType.IsAbstract);

            if (Implementations.ContainsKey(interfaceType))
                Implementations[interfaceType] = implementationType;
            else
                AddImplementation(interfaceType, implementationType);
        }
    }
}