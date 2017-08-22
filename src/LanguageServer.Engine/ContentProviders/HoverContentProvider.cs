using Lsp.Models;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.ContentProviders
{
    using MSBuild;
    using Documents;
    using Utilities;

    /// <summary>
    ///     Content for tooltips when hovering over nodes in the MSBuild XML.
    /// </summary>
    public class HoverContentProvider
    {
        /// <summary>
        ///     The project document for which hover content is provided.
        /// </summary>
        readonly ProjectDocument _projectDocument;

        /// <summary>
        ///     Create a new <see cref="HoverContentProvider"/>.
        /// </summary>
        /// <param name="projectDocument">
        ///     The project document for which hover content is provided.
        /// </param>
        public HoverContentProvider(ProjectDocument projectDocument)
        {
            if (projectDocument == null)
                throw new ArgumentNullException(nameof(projectDocument));
            
            _projectDocument = projectDocument;
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildProperty"/>.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="MSBuildProperty"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer Property(MSBuildProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            
            if (property.IsOverridden)
            {
                Position overridingDeclarationPosition = property.DeclaringXml.Location.ToNative();

                StringBuilder overrideDescription = new StringBuilder();
                string declarationFile = property.DeclaringXml.Location.File;
                if (declarationFile != property.Property.Xml.Location.File)
                {
                    Uri declarationDocumentUri = VSCodeDocumentUri.FromFileSystemPath(declarationFile);
                    overrideDescription.AppendLine(
                        $"Value overridden at {overridingDeclarationPosition} in [{Path.GetFileName(declarationFile)}]({declarationDocumentUri})."
                    );
                }
                else
                    overrideDescription.AppendLine($"Value overridden at {overridingDeclarationPosition} in this file.");

                overrideDescription.AppendLine();
                overrideDescription.AppendLine();
                overrideDescription.AppendLine(
                    $"Unused value: `{property.DeclaringXml.Value}`"
                );
                overrideDescription.AppendLine();
                overrideDescription.AppendLine(
                    $"Actual value: `{property.Value}`"
                );

                return new MarkedStringContainer(
                    $"Property: `{property.Name}`",
                    overrideDescription.ToString()
                );
            }

            return new MarkedStringContainer(
                $"Property: `{property.Name}`",
                $"Value: `{property.Value}`"
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildUnusedProperty"/>.
        /// </summary>
        /// <param name="undefinedProperty">
        ///     The <see cref="MSBuildUnusedProperty"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer UnusedProperty(MSBuildUnusedProperty undefinedProperty)
        {
            if (undefinedProperty == null)
                throw new ArgumentNullException(nameof(undefinedProperty));
 
            string condition = undefinedProperty.PropertyElement.Condition;
            if (String.IsNullOrWhiteSpace(condition))
                condition = undefinedProperty.PropertyElement.Parent.Condition; // Condition may be on parent element.

            string expandedCondition = _projectDocument.MSBuildProject.ExpandString(condition);

            return new MarkedStringContainer(
                $"Property: `{undefinedProperty.Name}` (condition evaluates to `false`)",
                $"Unused value:\n* `{undefinedProperty.Value}`\n\nCondition:\n* Raw =`{condition}`\n* Evaluated = `{expandedCondition}`"
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildItemGroup"/>.
        /// </summary>
        /// <param name="itemGroup">
        ///     The <see cref="MSBuildItemGroup"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer ItemGroup(MSBuildItemGroup itemGroup)
        {
            if (itemGroup == null)
                throw new ArgumentNullException(nameof(itemGroup));

            if (itemGroup.Name == "PackageReference")
            {
                string packageVersion = itemGroup.GetFirstMetadataValue("Version");
                
                return new MarkedStringContainer(
                    $"NuGet Package: `{itemGroup.FirstInclude}`",
                    $"Version: {packageVersion}"
                );
            }

            string[] includes = itemGroup.Includes.ToArray();
            StringBuilder itemIncludeContent = new StringBuilder();
            itemIncludeContent.AppendLine(
                $"Include: `{itemGroup.OriginatingElement.Include}`  "
            );
            itemIncludeContent.AppendLine();
            itemIncludeContent.Append(
                $"Evaluates to {itemGroup.Items.Count} item"
            );
            if (!itemGroup.HasSingleItem)
                itemIncludeContent.Append("s");
            itemIncludeContent.AppendLine(".");

            foreach (string include in includes.Take(5))
            {
                // TODO: Consider making hyperlinks for includes that map to files which exist.
                itemIncludeContent.AppendLine(
                    $"* `{include}`"
                );
            }
            if (includes.Length > 5)
                itemIncludeContent.AppendLine("* ...");

            return new MarkedStringContainer(
                $"Item Group: `{itemGroup.OriginatingElement.ItemType}`",
                itemIncludeContent.ToString()
            );  
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildUnusedItemGroup"/>.
        /// </summary>
        /// <param name="unusedItemGroup">
        ///     The <see cref="MSBuildUnusedItemGroup"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer UnusedItemGroup(MSBuildUnusedItemGroup unusedItemGroup)
        {
            if (unusedItemGroup == null)
                throw new ArgumentNullException(nameof(unusedItemGroup));
            
            string condition = unusedItemGroup.Condition;
            string evaluatedCondition = _projectDocument.MSBuildProject.ExpandString(condition);

            StringBuilder descriptionContent = new StringBuilder();
            descriptionContent.AppendLine(
                $"Condition: `{condition}`"
            );
            descriptionContent.AppendLine();
            descriptionContent.AppendLine(
                $"Evaluated Condition: `{evaluatedCondition}`"
            );
            descriptionContent.AppendLine();
            descriptionContent.AppendLine("---");
            descriptionContent.AppendLine();

            string[] includes = unusedItemGroup.Includes.ToArray();
            descriptionContent.AppendLine(
                $"Include: `{unusedItemGroup.OriginatingElement.Include}`  "
            );
            descriptionContent.AppendLine();
            descriptionContent.Append(
                $"Evaluates to {unusedItemGroup.Items.Count} item"
            );
            if (!unusedItemGroup.HasSingleItem)
                descriptionContent.Append("s");
            descriptionContent.AppendLine(".");

            foreach (string include in includes.Take(5))
            {
                // TODO: Consider making hyperlinks for includes that map to files which exist.
                descriptionContent.AppendLine(
                    $"* `{include}`"
                );
            }
            if (includes.Length > 5)
                descriptionContent.AppendLine("* ...");

            return new MarkedStringContainer(
                $"Unused Item Group: `{unusedItemGroup.OriginatingElement.ItemType}` (condition evaluates to `false`)",
                descriptionContent.ToString()
            );  
        }

        /// <summary>
        ///     Get hover content for an MSBuild condition.
        /// </summary>
        /// <param name="elementName">
        ///     The name of the element that contains the Condition attribute.
        /// </param>
        /// <param name="condition">
        ///     The raw (unevaluated) condition.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer Condition(string elementName, string condition)
        {
            if (String.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'elementName'.", nameof(elementName));

            if (String.IsNullOrWhiteSpace(condition))
                return null;

            string evaluatedCondition = _projectDocument.MSBuildProject.ExpandString(condition);
            
            return new MarkedStringContainer(
                "Condition",
                $"Evaluated: `{evaluatedCondition}`"
            );
        }

        /// <summary>
        ///     Get hover content for metadata of an <see cref="MSBuildItemGroup"/>.
        /// </summary>
        /// <param name="itemGroup">
        ///     The <see cref="MSBuildItemGroup"/>.
        /// </param>
        /// <param name="metadataName">
        ///     The metadata name.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer ItemGroupMetadata(MSBuildItemGroup itemGroup, string metadataName)
        {
            if (itemGroup == null)
                throw new ArgumentNullException(nameof(itemGroup));

            if (String.IsNullOrWhiteSpace(metadataName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'metadataName'.", nameof(metadataName));

            if (itemGroup.Name == "PackageReference")
                return ItemGroup(itemGroup);

            if (metadataName == "Condition")
                return Condition(itemGroup.Name, itemGroup.FirstItem.Xml.Condition);

            if (String.Equals(metadataName, "Include"))
                metadataName = "Identity";

            string[] metadataValues =
                itemGroup.GetMetadataValues(metadataName).Where(
                    value => !String.IsNullOrWhiteSpace(value)
                )
                .Distinct()
                .ToArray();

            StringBuilder metadataContent = new StringBuilder();
            if (metadataValues.Length > 0)
            {
                metadataContent.AppendLine("Values:");
                foreach (string metadataValue in metadataValues)
                {
                    metadataContent.AppendLine(
                        $"* `{metadataValue}`"
                    );
                }
            }
            else
                metadataContent.AppendLine("No values are present for this metadata.");

            return new MarkedStringContainer(
                $"Item Metadata: `{itemGroup.Name}.{metadataName}`",
                metadataContent.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for a metadata attribute of an <see cref="MSBuildUnusedItemGroup"/>.
        /// </summary>
        /// <param name="itemGroup">
        ///     The <see cref="MSBuildUnusedItemGroup"/>.
        /// </param>
        /// <param name="metadataName">
        ///     The name of the metadata attribute.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer UnusedItemGroupMetadata(MSBuildUnusedItemGroup itemGroup, string metadataName)
        {
            if (itemGroup == null)
                throw new ArgumentNullException(nameof(itemGroup));

            if (String.IsNullOrWhiteSpace(metadataName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'metadataName'.", nameof(metadataName));

            if (itemGroup.Name == "PackageReference")
                return UnusedItemGroup(itemGroup);

            if (metadataName == "Condition")
                return Condition(itemGroup.Name, itemGroup.FirstItem.Xml.Condition);

            if (String.Equals(metadataName, "Include"))
                metadataName = "Identity";

            string[] metadataValues =
                itemGroup.GetMetadataValues(metadataName).Where(
                    value => !String.IsNullOrWhiteSpace(value)
                )
                .Distinct()
                .ToArray();

            StringBuilder metadataContent = new StringBuilder();
            if (metadataValues.Length > 0)
            {
                metadataContent.AppendLine("Values:");
                foreach (string metadataValue in metadataValues)
                {
                    metadataContent.AppendLine(
                        $"* `{metadataValue}`"
                    );
                }
            }
            else
                metadataContent.AppendLine("No values are present for this metadata.");

            return new MarkedStringContainer(
                $"Unused Item Metadata: `{itemGroup.Name}.{metadataName}` (item condition evaluates to `false`)",
                metadataContent.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildTarget"/>.
        /// </summary>
        /// <param name="target">
        ///     The <see cref="MSBuildTarget"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer Target(MSBuildTarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
            return $"Target: `{target.Name}`";
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildImport"/>.
        /// </summary>
        /// <param name="import">
        ///     The <see cref="MSBuildImport"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer Import(MSBuildImport import)
        {
            if (import == null)
                throw new ArgumentNullException(nameof(import));
            
            StringBuilder imports = new StringBuilder("Imports:");
            imports.AppendLine();
            foreach (string projectFile in import.ImportedProjectFiles)
                imports.AppendLine($"* [{Path.GetFileName(projectFile)}]({VSCodeDocumentUri.FromFileSystemPath(projectFile)})");

            return new MarkedStringContainer(
                $"Import: `{import.Name}`",
                imports.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildImport"/>.
        /// </summary>
        /// <param name="unresolvedImport">
        ///     The <see cref="MSBuildImport"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer UnresolvedImport(MSBuildUnresolvedImport unresolvedImport)
        {
            if (unresolvedImport == null)
                throw new ArgumentNullException(nameof(unresolvedImport));

            string condition = unresolvedImport.Condition;
            string evaluatedCondition = _projectDocument.MSBuildProject.ExpandString(condition);

            string project = unresolvedImport.Project;
            string evaluatedProject = _projectDocument.MSBuildProject.ExpandString(project);

            StringBuilder descriptionContent = new StringBuilder();
            descriptionContent.AppendLine(
                $"Project: `{project}`"
            );
            descriptionContent.AppendLine();
            descriptionContent.AppendLine(
                $"Evaluated Project: `{evaluatedProject}`"
            );
            descriptionContent.AppendLine();
            descriptionContent.AppendLine(
                $"Condition: `{condition}`"
            );
            descriptionContent.AppendLine();
            descriptionContent.AppendLine(
                $"Evaluated Condition: `{evaluatedCondition}`"
            );

            return new MarkedStringContainer(
                $"Unresolved Import (condition evaluates to `false`)",
                descriptionContent.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildSdkImport"/>.
        /// </summary>
        /// <param name="sdkImport">
        ///     The <see cref="MSBuildSdkImport"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer SdkImport(MSBuildSdkImport sdkImport)
        {
            if (sdkImport == null)
                throw new ArgumentNullException(nameof(sdkImport));
            
            StringBuilder imports = new StringBuilder("Imports:");
            imports.AppendLine();
            foreach (string projectFile in sdkImport.ImportedProjectFiles)
                imports.AppendLine($"* [{Path.GetFileName(projectFile)}]({VSCodeDocumentUri.FromFileSystemPath(projectFile)})");

            return new MarkedStringContainer(
                $"SDK Import: {sdkImport.Name}",
                imports.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildUnresolvedSdkImport"/>.
        /// </summary>
        /// <param name="unresolvedSdkImport">
        ///     The <see cref="MSBuildUnresolvedSdkImport"/>.
        /// </param>
        /// <returns>
        ///     The content, or <c>null</c> if no content is provided.
        /// </returns>
        public MarkedStringContainer UnresolvedSdkImport(MSBuildUnresolvedSdkImport unresolvedSdkImport)
        {
            if (unresolvedSdkImport == null)
                throw new ArgumentNullException(nameof(unresolvedSdkImport));
            
            string condition = unresolvedSdkImport.Condition;
            string evaluatedCondition = _projectDocument.MSBuildProject.ExpandString(condition);

            StringBuilder descriptionContent = new StringBuilder();
            descriptionContent.AppendLine(
                $"Condition: `{condition}`"
            );
            descriptionContent.AppendLine();
            descriptionContent.AppendLine(
                $"Evaluated Condition: `{evaluatedCondition}`"
            );

            return new MarkedStringContainer(
                $"Unresolved Import `{unresolvedSdkImport.Sdk}` (condition evaluates to `false`)",
                descriptionContent.ToString()
            );
        }
    }
}
