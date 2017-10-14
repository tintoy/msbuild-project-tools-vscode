using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Equality comparer for <see cref="ProjectUsingTaskElement"/>s that compares <see cref="ProjectUsingTaskElement.AssemblyFile"/>.
    /// </summary>
    class UsingTaskAssemblyEqualityComparer
        : EqualityComparer<ProjectUsingTaskElement>
    {
        /// <summary>
        ///     The singleton instance of the <see cref="UsingTaskAssemblyEqualityComparer"/>.
        /// </summary>
        public static readonly UsingTaskAssemblyEqualityComparer Instance = new UsingTaskAssemblyEqualityComparer();

        /// <summary>
        ///     Create a new <see cref="UsingTaskAssemblyEqualityComparer"/>.
        /// </summary>
        UsingTaskAssemblyEqualityComparer()
        {
        }

        /// <summary>
        ///     Determine whether 2 <see cref="UsingTaskAssemblyEqualityComparer"/>s are equal.
        /// </summary>
        /// <param name="usingTask1">
        ///     The first <see cref="UsingTaskAssemblyEqualityComparer"/>.
        /// </param>
        /// <param name="usingTask2">
        ///     The second <see cref="UsingTaskAssemblyEqualityComparer"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the <see cref="UsingTaskAssemblyEqualityComparer"/>s are equal; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(ProjectUsingTaskElement usingTask1, ProjectUsingTaskElement usingTask2) => String.Equals(usingTask1?.AssemblyFile, usingTask2?.AssemblyFile, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        ///     Get a hash code to represent the specified <see cref="UsingTaskAssemblyEqualityComparer"/>.
        /// </summary>
        /// <param name="usingTask">
        ///     The <see cref="UsingTaskAssemblyEqualityComparer"/>.
        /// </param>
        /// <returns>
        ///     The hash code.
        /// </returns>
        public override int GetHashCode(ProjectUsingTaskElement usingTask) => usingTask.AssemblyFile?.GetHashCode() ?? 0;
    }
}
