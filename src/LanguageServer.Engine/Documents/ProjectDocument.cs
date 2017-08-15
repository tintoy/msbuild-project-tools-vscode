using System;
using System.IO;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.Documents
{
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
        ///     Create a new <see cref="ProjectDocument"/>.
        /// </summary>
        /// <param name="projectFilePath">
        ///     The full path to the project file.
        /// </param>
        public ProjectDocument(string projectFilePath)
        {
            if (String.IsNullOrWhiteSpace(projectFilePath))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'projectFilePath'.", nameof(projectFilePath));
            
            _projectFile = new FileInfo(projectFilePath);
        }

        /// <summary>
        ///     Is the project currently loaded?
        /// </summary>
        public bool IsLoaded => _xml != null && _lookup != null;

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
        ///     Load and parse the project.
        /// </summary>
        public void Load()
        {
            _xml = LocatingXmlTextReader.LoadWithLocations(_projectFile.FullName);
            _lookup = new PositionalObjectLookup(_xml);
        }

        public void Unload()
        {
            _xml = null;
            _lookup = null;
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
    }
}
