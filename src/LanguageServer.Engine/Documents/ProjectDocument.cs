using Nito.AsyncEx;
using Serilog;
using System;
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
        ///     The parsed project XML.
        /// </summary>
        XDocument _xml;

        /// <summary>
        ///     The lookup for XML objects by position.
        /// </summary>
        PositionalObjectLookup _lookup;

        /// <summary>
        ///     The underlying MSBuild project collection.
        /// </summary>
        MSBuild.ProjectCollection _msbuildProjectCollection;

        /// <summary>
        ///     The underlying MSBuild project.
        /// </summary>
        MSBuild.Project _msbuildProject;

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
        public bool IsLoaded => _xml != null && _lookup != null;

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
        ///     The project object-lookup facility.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     The project is not loaded.
        /// </exception>
        public PositionalObjectLookup Lookup => _lookup ?? throw new InvalidOperationException("Project is not loaded.");

        /// <summary>
        ///     The underlying MSBuild project (if any).
        /// </summary>
        public MSBuild.Project MSBuildProject => _msbuildProject;

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
            _lookup = new PositionalObjectLookup(_xml);
            TryLoadMSBuildProject();
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
            _lookup = new PositionalObjectLookup(_xml);
            IsDirty = true;
            
            TryLoadMSBuildProject();
        }

        /// <summary>
        ///     Unload the project.
        /// </summary>
        public void Unload()
        {
            TryUnloadMSBuildProject();
            _xml = null;
            _lookup = null;
            IsDirty = false;
        }

        /// <summary>
        ///     Get the XML object (if any) at the specified position.
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

            return _lookup.Find(
                position.ToOneBased()
            );
        }

        /// <summary>
        ///     Get the XML object (if any) at the specified position.
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
