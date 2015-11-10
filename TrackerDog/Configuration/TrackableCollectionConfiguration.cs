namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents a configuration to setup how collection change tracking will behave.
    /// </summary>
    public sealed class TrackableCollectionConfiguration
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TrackableCollectionConfiguration()
        {
            AddImplementation(typeof(ISet<>), typeof(HashSet<>));
            AddImplementation(typeof(IList<>), typeof(List<>));
            AddImplementation(typeof(ICollection<>), typeof(List<>));
        }

        /// <summary>
        /// Gets a dictionary of implementations to common collection interfaces.
        /// </summary>
        internal Dictionary<Type, Type> Implementations { get; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Gets the implementation of a given type. The given type can or cannot be a collection interface, but
        /// it can be also an actual collection type. This method will return the most appropiate generic collection
        /// implementation to the given type.
        /// </summary>
        /// <param name="some">The whole collection type</param>
        /// <returns>A pair, where the key is the collection interface and value is the collection implementation</returns>
        public KeyValuePair<Type, Type> GetImplementation(Type some)
        {
            Type someGenericTypeDefinition = some.GetGenericTypeDefinition();
            IEnumerable<Type> someInterfaces = some.GetInterfaces();

            KeyValuePair<Type, Type> result = Implementations.FirstOrDefault
            (
                interfaceType =>
                    some.IsGenericType && someGenericTypeDefinition == interfaceType.Key
                    ||
                    someInterfaces.Any
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

        /// <summary>
        /// Adds a new implementation to some collection interface. This method will not support replacing
        /// an already added interface/implementation pair.
        /// </summary>
        /// <param name="interfaceType">The collection interface</param>
        /// <param name="implementationType">The collection implementation</param>
        public void AddImplementation(Type interfaceType, Type implementationType)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(interfaceType.IsInterface);
            Contract.Requires(interfaceType.IsGenericTypeDefinition);
            Contract.Requires(implementationType != null);
            Contract.Requires(implementationType.IsClass && !implementationType.IsAbstract);
            Contract.Requires(implementationType.IsGenericTypeDefinition);

            Contract.Assert(!Implementations.ContainsKey(interfaceType));

            Implementations.Add(interfaceType, implementationType);
        }

        /// <summary>
        /// Replaces an existing collection interface/implementation or adds it.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implementationType"></param>
        public void ReplaceImplementation(Type interfaceType, Type implementationType)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(interfaceType.IsInterface);
            Contract.Requires(interfaceType.IsGenericTypeDefinition);
            Contract.Requires(implementationType != null);
            Contract.Requires(implementationType.IsClass && !implementationType.IsAbstract);
            Contract.Requires(implementationType.IsGenericTypeDefinition);

            if (Implementations.ContainsKey(interfaceType))
                Implementations[interfaceType] = implementationType;
            else
                AddImplementation(interfaceType, implementationType);
        }
    }
}