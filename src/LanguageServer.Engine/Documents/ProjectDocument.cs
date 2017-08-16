using Nito.AsyncEx;
using NuGet.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using MSBuild = Microsoft.Build.Evaluation;

namespace MSBuildProjectTools.LanguageServer.Documents
{
    using Utilities;
    using XmlParser;

    /// <summary>
    ///     Represents the document state for an MSBuild project.
    /// </summary>
    public class ProjectDocument
    {
        /// <summary>
        ///     The project file.
        /// </summary>
        readonly FileInfo _projectFile;

        /// <summary>
        ///     The project's configured package sources.
        /// </summary>
        readonly List<PackageSource> _configuredPackageSources = new List<PackageSource>();

        /// <summary>
        ///     The parsed project XML.
        /// </summary>
        XDocument _xml;

        /// <summary>
        ///     The lookup for XML objects by position.
        /// </summary>
        PositionalXmlObjectLookup _xmlLookup;

        /// <summary>
        ///     The underlying MSBuild project collection.
        /// </summary>
        MSBuild.ProjectCollection _msbuildProjectCollection;

        /// <summary>
        ///     The underlying MSBuild project.
        /// </summary>
        MSBuild.Project _msbuildProject;

        /// <summary>
        ///     The lookup for MSBuild objects by position.
        /// </summary>
        PositionalMSBuildLookup _msbuildLookup;

        /// <summary>
        ///     Create a new <see cref="ProjectDocument"/>.
        /// </summary>
        /// <param name="projectFilePath">
        ///     The full path to the project file.
        /// </param>
        public ProjectDocument(string projectFilePath, ILogger logger)
        {
            if (String.IsNullOrWhiteSpace(projectFilePath))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'projectFilePath'.", nameof(projectFilePath));
            
            _projectFile = new FileInfo(projectFilePath);
            Log = logger.ForContext("ProjectDocument", _projectFile.FullName);
        }

        /// <summary>
        ///     A lock used to control access to project state.
        /// </summary>
        public AsyncReaderWriterLock Lock { get; } = new AsyncReaderWriterLock();

        /// <summary>
        ///     Is the project currently loaded?
        /// </summary>
        public bool IsLoaded => _xml != null && _xmlLookup != null;

        /// <summary>
        ///     Does the project have in-memory changes?
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        ///     Is the underlying MSBuild project currently loaded?
        /// </summary>
        public bool HaveMSBuildProject => _msbuildProject != null;

        /// <summary>
        ///     The parsed project XML.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     The project is not loaded.
        /// </exception>
        /// <remarks>
        ///     Do not modify this <see cref="XDocument"/>.
        /// </remarks>
        public XDocument Xml => _xml ?? throw new InvalidOperationException("Project is not loaded.");

        /// <summary>
        ///     The project XML object lookup facility.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     The project is not loaded.
        /// </exception>
        public PositionalXmlObjectLookup XmlLookup => _xmlLookup ?? throw new InvalidOperationException("Project is not loaded.");

        /// <summary>
        ///     The project MSBuild object-lookup facility.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     The project is not loaded.
        /// </exception>
        public PositionalMSBuildLookup MSBuildLookup => _msbuildLookup ?? throw new InvalidOperationException("MSBuild project is not loaded.");

        /// <summary>
        ///     The underlying MSBuild project (if any).
        /// </summary>
        public MSBuild.Project MSBuildProject => _msbuildProject;

        /// <summary>
        ///     NuGet package sources configured for the current project.
        /// </summary>
        public IReadOnlyList<PackageSource> ConfiguredPackageSources => _configuredPackageSources;

        /// <summary>
        ///     The document's logger.
        /// </summary>
        ILogger Log { get; set; }

        /// <summary>
        ///     Load and parse the project.
        /// </summary>
        public void Load()
        {
            _xml = Parser.Load(_projectFile.FullName);
            _xmlLookup = new PositionalXmlObjectLookup(_xml);
            TryLoadMSBuildProject();
            RefreshPackageSources();
            IsDirty = false;
        }

        /// <summary>
        ///     Update the project in-memory state.
        /// </summary>
        /// <param name="xml">
        ///     The project XML.
        /// </param>
        public void Update(string xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            _xml = Parser.Parse(xml);
            _xmlLookup = new PositionalXmlObjectLookup(_xml);
            IsDirty = true;
            
            TryLoadMSBuildProject();
        }

        /// <summary>
        ///     Determine the NuGet package sources configured for the current project.
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if the package sources were loaded; otherwise, <c>false</c>.
        /// </returns>
        public bool RefreshPackageSources()
        {
            try
            {
                _configuredPackageSources.Clear();
                _configuredPackageSources.AddRange(
                    NuGetHelper.GetWorkspacePackageSources(_projectFile.Directory.FullName)
                );

                return true;
            }
            catch (Exception packageSourceLoadError)
            {
                Log.Error(packageSourceLoadError, "Error configuring NuGet package sources for MSBuild project '{ProjectFileName}'.", _projectFile.FullName);

                return false;
            }
        }

        /// <summary>
        ///     Unload the project.
        /// </summary>
        public void Unload()
        {
            TryUnloadMSBuildProject();

            _xml = null;
            _xmlLookup = null;
            IsDirty = false;
        }

        /// <summary>
        ///     Get the XML object (if any) at the specified position in the project file.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     The object, or <c>null</c> no object was found at the specified position.
        /// </returns>
        public XObject GetXmlAtPosition(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (!IsLoaded)
                throw new InvalidOperationException("Project is not loaded.");

            return _xmlLookup.Find(
                position.ToOneBased()
            );
        }

        /// <summary>
        ///     Get the XML object (if any) at the specified position in the project file.
        /// </summary>
        /// <typeparam name="TXml">
        ///     The type of XML object to return.
        /// </typeparam>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     The object, or <c>null</c> no object of the specified type was found at the specified position.
        /// </returns>
        public TXml GetXmlAtPosition<TXml>(Position position)
            where TXml : XObject
        {
            return GetXmlAtPosition(position) as TXml;
        }

        /// <summary>
        ///     Get the MSBuild object (if any) at the specified position in the project file.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     The MSBuild object, or <c>null</c> no object was found at the specified position.
        /// </returns>
        public object GetMSBuildObjectAtPosition(Position position)
        {
            if (!HaveMSBuildProject)
                throw new InvalidOperationException("Project is not loaded.");

            return _msbuildLookup.Find(position);
        }

        /// <summary>
        ///     Attempt to load the underlying MSBuild project.
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if the project was successfully loaded; otherwise, <c>false</c>.
        /// </returns>
        bool TryLoadMSBuildProject()
        {
            try
            {
                if (HaveMSBuildProject && !IsDirty)
                    return true;

                if (_msbuildProjectCollection == null)
                    _msbuildProjectCollection = MSBuildHelper.CreateProjectCollection(_projectFile.Directory.FullName);

                if (HaveMSBuildProject && IsDirty)
                {
                    _msbuildProject.Xml.ReloadFrom(
                        reader: _xml.CreateReader(),
                        throwIfUnsavedChanges: false
                    );

                    Log.Verbose("Successfully updated MSBuild project '{ProjectFileName}' from in-memory changes.");
                }
                else
                    _msbuildProject = _msbuildProjectCollection.LoadProject(_projectFile.FullName);

                _msbuildLookup = new PositionalMSBuildLookup(_msbuildProject, _xmlLookup);

                return true;
            }
            catch (Exception loadError)
            {
                Log.Error(loadError, "Error loading MSBuild project '{ProjectFileName}'.", _projectFile.FullName);

                return false;
            }
        }

        /// <summary>
        ///     Attempt to unload the underlying MSBuild project.
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if the project was successfully unloaded; otherwise, <c>false</c>.
        /// </returns>
        bool TryUnloadMSBuildProject()
        {
            try
            {
                if (!HaveMSBuildProject)
                    return true;

                if (_msbuildProjectCollection == null)
                    return true;

                _msbuildLookup = null;
                _msbuildProjectCollection.UnloadProject(_msbuildProject);
                _msbuildProject = null;

                return true;
            }
            catch (Exception unloadError)
            {
                Log.Error(unloadError, "Error unloading MSBuild project '{ProjectFileName}'.", _projectFile.FullName);

                return false;
            }
        }
    }
}
