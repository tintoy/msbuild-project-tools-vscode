using Microsoft.Language.Xml;
using System;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    /// <summary>
    ///     An object in an MSBuild project.
    /// </summary>
    public abstract class MSBuildObject
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildObject"/>.
        /// </summary>
        /// <param name="xml">
        ///     A <see cref="SyntaxNode"/> representing the item's corresponding XML.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of text covered by the item's XML.
        /// </param>
        protected MSBuildObject(SyntaxNode xml, Range xmlRange)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            if (xmlRange == null)
                throw new ArgumentNullException(nameof(xmlRange));
            
            Xml = xml;
            XmlRange = xmlRange;
        }

        /// <summary>
        ///     A <see cref="SyntaxNode"/> representing the item's corresponding XML.
        /// </summary>
        public SyntaxNode Xml { get; }

        /// <summary>
        ///     A <see cref="Range"/> representing the span of text covered by the item's XML.
        /// </summary>
        public Range XmlRange { get; }

        /// <summary>
        ///     The object's name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildObject"/>.
        /// </summary>
        public abstract MSBuildObjectKind Kind { get; }

        /// <summary>
        ///     The full path of the file where the object is declared.
        /// </summary>
        public abstract string SourceFile { get; }

        /// <summary>
        ///     Determine whether the object's XML contains the specified position.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the the specified position lies within the object's XML span; otherwise, <c>false</c>.
        /// </returns>
        public bool XmlContains(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            
            return XmlRange.Contains(position);
        }
    }

    /// <summary>
    ///     An object of a known type in an MSBuild project.
    /// </summary>
    /// <typeparam name="TUnderlyingObject">
    ///     The type of underlying object represented by the <see cref="MSBuildObject{TUnderlyingObject}"/>.
    /// </typeparam>
    public abstract class MSBuildObject<TUnderlyingObject>
        : MSBuildObject
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildObject{TUnderlyingObject}"/>.
        /// </summary>
        /// <param name="underlyingObject">
        ///     The underlying MSBuild object.
        /// </param>
        /// <param name="declaringXml">
        ///     A <see cref="SyntaxNode"/> representing the object's declaring XML.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of text covered by the item's XML.
        /// </param>
        protected MSBuildObject(TUnderlyingObject underlyingObject, SyntaxNode declaringXml, Range xmlRange)
            : base(declaringXml, xmlRange)
        {
            if (underlyingObject == null)
                throw new ArgumentNullException(nameof(underlyingObject));
            
            UnderlyingObject = underlyingObject;
        }

        /// <summary>
        ///     The underlying MSBuild object.
        /// </summary>
        protected TUnderlyingObject UnderlyingObject { get; }
    }
}
