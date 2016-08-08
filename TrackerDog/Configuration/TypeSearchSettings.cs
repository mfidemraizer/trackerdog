using System;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Defines specific settings when supporting operations search types
    /// during configuration stages.
    /// </summary>
    public sealed class TypeSearchSettings
    {
        /// <summary>
        /// Gets or sets a filter to include or exclude types from a given search.
        /// </summary>
        /// <example>
        /// <para>For example the following filter would restrict the change-tracking behavior to
        /// just types that derives a class called <codeInline>DomainObject</codeInline></para>
        /// <code language="c#">
        /// settings.Filter = t => typeof(DomainObject).IsAssignableFrom(t);
        /// </code>
        /// </example>
        public Func<Type, bool> Filter { get; set; }

        /// <summary>
        /// Gets or sets a flag to determine if types should be configured recursively.
        /// </summary>
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets search mode. See <see cref="TypeSearchMode"/> to learn more about available modes.
        /// </summary>
        public TypeSearchMode Mode { get; set; } = TypeSearchMode.All;
    }
}
