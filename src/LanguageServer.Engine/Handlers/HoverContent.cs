using Lsp.Models;
using Microsoft.Language.Xml;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using MSBuild;
    using Documents;
    using Utilities;

    /// <summary>
    ///     Content for tooltips when hovering over nodes in the MSBuild XML.
    /// </summary>
    public static class HoverContent
    {
        /// <summary>
        ///     Get hover content for an <see cref="MSBuildProperty"/>.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="MSBuildProperty"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        public static MarkedStringContainer Property(MSBuildProperty property)
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
                    Uri declarationDocumentUri = UriHelper.CreateDocumentUri(declarationFile);
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
        ///     The content.
        /// </returns>
        public static MarkedStringContainer UnusedProperty(MSBuildUnusedProperty undefinedProperty, ProjectDocument projectDocument)
        {
            if (undefinedProperty == null)
                throw new ArgumentNullException(nameof(undefinedProperty));
 
            string condition = undefinedProperty.PropertyElement.Condition;
            if (String.IsNullOrWhiteSpace(condition))
                condition = undefinedProperty.PropertyElement.Parent.Condition; // Condition may be on parent element.

            string expandedCondition = projectDocument.MSBuildProject.ExpandString(condition);

            return new MarkedStringContainer(
                $"Property: `{undefinedProperty.Name}` (condition evaluates to false)",
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
        ///     The content.
        /// </returns>
        public static MarkedStringContainer ItemGroup(MSBuildItemGroup itemGroup)
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
                $"Items Group: `{itemGroup.OriginatingElement.ItemType}`",
                itemIncludeContent.ToString()
            );  
        }

        /// <summary>
        ///     Get hover content for an attribute of an <see cref="MSBuildItemGroup"/>.
        /// </summary>
        /// <param name="itemGroup">
        ///     The <see cref="MSBuildItemGroup"/>.
        /// </param>
        /// <param name="attribute">
        ///     The attribute.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        public static MarkedStringContainer ItemGroup(MSBuildItemGroup itemGroup, XmlAttributeSyntax attribute)
        {
            if (itemGroup == null)
                throw new ArgumentNullException(nameof(itemGroup));

            if (itemGroup.Name == "PackageReference")
                return ItemGroup(itemGroup);

            // TODO: Handle the "Condition" attribute.
            if (attribute.Name == "Condition")
                return null;

            string metadataName = attribute.Name;
            if (String.Equals(metadataName, "Include"))
                metadataName = "Identity";

            StringBuilder metadataValues = new StringBuilder();
            metadataValues.AppendLine("Values:");

            foreach (string metadataValue in itemGroup.GetMetadataValues(metadataName).Distinct())
            {
                metadataValues.AppendLine(
                    $"* `{metadataValue}`"
                );
            }

            return new MarkedStringContainer(
                $"Item Metadata: `{itemGroup.Name}.{metadataName}`",
                metadataValues.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildTarget"/>.
        /// </summary>
        /// <param name="target">
        ///     The <see cref="MSBuildTarget"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        public static MarkedStringContainer Target(MSBuildTarget target)
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
        ///     The content.
        /// </returns>
        public static MarkedStringContainer Import(MSBuildImport import)
        {
            if (import == null)
                throw new ArgumentNullException(nameof(import));
            
            StringBuilder imports = new StringBuilder("Imports:");
            imports.AppendLine();
            foreach (string projectFile in import.ImportedProjectFiles)
                imports.AppendLine($"* [{Path.GetFileName(projectFile)}]({UriHelper.CreateDocumentUri(projectFile)})");

            return new MarkedStringContainer(
                $"Import: {import.Name}",
                imports.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildSdkImport"/>.
        /// </summary>
        /// <param name="sdkImport">
        ///     The <see cref="MSBuildSdkImport"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        public static MarkedStringContainer SdkImport(MSBuildSdkImport sdkImport)
        {
            if (sdkImport == null)
                throw new ArgumentNullException(nameof(sdkImport));
            
            StringBuilder imports = new StringBuilder("Imports:");
            imports.AppendLine();
            foreach (string projectFile in sdkImport.ImportedProjectFiles)
                imports.AppendLine($"* [{Path.GetFileName(projectFile)}]({UriHelper.CreateDocumentUri(projectFile)})");

            return new MarkedStringContainer(
                $"SDK Import: {sdkImport.Name}",
                imports.ToString()
            );
        }
    }
}
