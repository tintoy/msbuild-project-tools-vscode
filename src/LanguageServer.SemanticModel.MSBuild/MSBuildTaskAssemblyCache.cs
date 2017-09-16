using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents cached data for MSBuild task assemblies.
    /// </summary>
    public sealed class MSBuildTaskAssemblyCache
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildTaskAssemblyCache"/>.
        /// </summary>
        public MSBuildTaskAssemblyCache()
        {
        }

        /// <summary>
        ///     A lock used to synchronise access to cache state.
        /// </summary>
        [JsonIgnore]
        public AsyncLock StateLock { get; } = new AsyncLock();

        /// <summary>
        ///     Metadata for assemblies, keyed by the assembly's full path.
        /// </summary>
        [JsonProperty("assemblies", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public Dictionary<string, MSBuildTaskAssemblyMetadata> Assemblies = new Dictionary<string, MSBuildTaskAssemblyMetadata>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     Get metadata for the specified task assembly, updating the cache if required.
        /// </summary>
        /// <param name="assemblyPath">
        ///     The full path to the assembly.
        /// </param>
        /// <returns>
        ///     The assembly metadata.
        /// </returns>
        public async Task<MSBuildTaskAssemblyMetadata> GetAssemblyMetadata(string assemblyPath)
        {
            if (String.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'assemblyPath'.", nameof(assemblyPath));
            
            MSBuildTaskAssemblyMetadata metadata;
            using (await StateLock.LockAsync())
            {
                FileInfo assemblyFile = new FileInfo(assemblyPath);
                if (!Assemblies.TryGetValue(assemblyPath, out metadata) || metadata.TimestampUtc < assemblyFile.LastWriteTimeUtc)
                {
                    metadata = await MSBuildTaskScanner.GetAssemblyTaskMetadata(assemblyPath);
                    Assemblies[metadata.AssemblyPath] = metadata;
                }
            }
            
            return metadata;
        }

        /// <summary>
        ///     Flush the cache.
        /// </summary>
        public void Flush()
        {
            using (StateLock.Lock())
            {
                Assemblies.Clear();
            }
        }

        /// <summary>
        ///     Load cache state from the specified file.
        /// </summary>
        /// <param name="cacheFile">
        ///     The file containing persisted cache state.
        /// </param>
        public void Load(string cacheFile)
        {
            if (String.IsNullOrWhiteSpace(cacheFile))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'cacheFile'.", nameof(cacheFile));
            
            using (StateLock.Lock())
            {
                Assemblies.Clear();

                using (StreamReader input = File.OpenText(cacheFile))
                using (JsonTextReader json = new JsonTextReader(input))
                {
                    new JsonSerializer().Populate(json, this);
                }
            }
        }

        /// <summary>
        ///     Write cache state to the specified file.
        /// </summary>
        /// <param name="cacheFile">
        ///     The file that will contain cache state.
        /// </param>
        public void Save(string cacheFile)
        {
            if (String.IsNullOrWhiteSpace(cacheFile))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'cacheFile'.", nameof(cacheFile));

            using (StateLock.Lock())
            {
                if (File.Exists(cacheFile))
                    File.Delete(cacheFile);

                using (StreamWriter output = File.CreateText(cacheFile))
                using (JsonTextWriter json = new JsonTextWriter(output))
                {
                    new JsonSerializer().Serialize(json, this);
                }
            }
        }        

        /// <summary>
        ///     Create a <see cref="MSBuildTaskAssemblyCache"/> using the state persisted in the specified file.
        /// </summary>
        /// <param name="cacheFile">
        ///     The file containing persisted cache state.
        /// </param>
        /// <returns>
        ///     The new <see cref="MSBuildTaskAssemblyCache"/>.
        /// </returns>
        public static MSBuildTaskAssemblyCache FromCacheFile(string cacheFile)
        {
            if (String.IsNullOrWhiteSpace(cacheFile))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'cacheFile'.", nameof(cacheFile));
            
            MSBuildTaskAssemblyCache cache = new MSBuildTaskAssemblyCache();
            cache.Load(cacheFile);
            
            return cache;
        }
    }
}
