using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace MSBuildProjectTools.LanguageServer.TaskReflection
{
    /// <summary>
    ///     An <see cref="AssemblyLoadContext"/> that loads assemblies from the specified directory.
    /// </summary>
    public class DirectoryAssemblyLoadContext
        : AssemblyLoadContext
    {
        /// <summary>
        ///     Create a new <see cref="DirectoryAssemblyLoadContext"/>.
        /// </summary>
        /// <param name="baseDirectory">
        ///     The base directory from which assemblies are loaded.
        /// </param>
        /// <param name="fallbackDirectory">
        ///     An optional directory from which assemblies are loaded if they are not found in the base directory.
        /// </param>
        public DirectoryAssemblyLoadContext(string baseDirectory, string fallbackDirectory = null)
        {
            if (String.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException($"Argument cannot be null, empty, or entirely composed of whitespace: {nameof(baseDirectory)}.", nameof(baseDirectory));

            BaseDirectory = new DirectoryInfo(baseDirectory);
            if (!String.IsNullOrWhiteSpace(fallbackDirectory))
                FallbackDirectory = new DirectoryInfo(fallbackDirectory);
            else
                FallbackDirectory = BaseDirectory;
        }

        /// <summary>
        ///     The base directory from which assemblies are loaded.
        /// </summary>
        public DirectoryInfo BaseDirectory { get; }

        /// <summary>
        ///     The directory from which assemblies are loaded if they are not found in the base directory.
        /// </summary>
        public DirectoryInfo FallbackDirectory { get; }

        /// <summary>
        ///     Load an assembly.
        /// </summary>
        /// <param name="assemblyName">
        ///     The assembly name.
        /// </param>
        /// <returns>
        ///     The assembly, or <c>null</c> if the assembly could not be loaded.
        /// </returns>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            Assembly assembly = LoadFromDirectory(assemblyName, BaseDirectory);
            if (assembly == null && FallbackDirectory != BaseDirectory)
                assembly = LoadFromDirectory(assemblyName, FallbackDirectory);

            return assembly;
        }

        /// <summary>
        ///     Load the specified assembly from a file in a directory.
        /// </summary>
        /// <param name="assemblyName">
        ///     The assembly name.
        /// </param>
        /// <param name="directory">
        ///     The directory to load the assembly from.
        /// </param>
        /// <returns>
        ///     The assembly, or <c>null</c> if no matching assembly file was found <paramref name="directory"/>.
        /// </returns>
        Assembly LoadFromDirectory(AssemblyName assemblyName, DirectoryInfo directory)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            string assemblyFile = Path.Combine(
                directory.FullName,
                assemblyName.Name + ".dll"
            );
            if (File.Exists(assemblyFile))
                return LoadFromAssemblyPath(assemblyFile);

            assemblyFile = Path.ChangeExtension(assemblyFile, ".exe");
            if (File.Exists(assemblyFile))
                return LoadFromAssemblyPath(assemblyFile);

            return null;
        }
    }
}
