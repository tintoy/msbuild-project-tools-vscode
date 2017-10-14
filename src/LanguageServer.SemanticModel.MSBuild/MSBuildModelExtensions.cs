using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Extension methods for working with MSBuild models.
    /// </summary>
    public static class MSBuildModelExtensions
    {
        /// <summary>
        ///     Enumerate the names of all tasks available for use in the project.
        /// </summary>
        /// <param name="project">
        ///     The MSBuild <see cref="Project"/>.
        /// </param>
        /// <returns>
        ///     A sequence of task names.
        /// </returns>
        public static IEnumerable<string> GetAvailableTaskNames(this Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            return project.GetAllUsingTasks().Select(
                usingTask => usingTask.TaskName
            );
        }

        /// <summary>
        ///     Recursively enumerate all <see cref="ProjectUsingTaskElement"/>s in the project and any projects that it imports.
        /// </summary>
        /// <param name="project">
        ///     The MSBuild <see cref="Project"/>.
        /// </param>
        /// <returns>
        ///     A sequence of <see cref="ProjectUsingTaskElement"/>s.
        /// </returns>
        public static IEnumerable<ProjectUsingTaskElement> GetAllUsingTasks(this Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            return
                project.Xml.UsingTasks.Concat(
                    project.Imports.SelectMany(
                        import => import.ImportedProject.UsingTasks
                    )
                );
        }


        /// <summary>
        ///     Convert the MSBuild <see cref="ElementLocation"/> to its native equivalent.
        /// </summary>
        /// <param name="location">
        ///     The <see cref="ElementLocation"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Position"/>.
        /// </returns>
        public static Position ToNative(this ElementLocation location)
        {
            if (location == null)
                return Position.Zero;

            if (location.Line == 0)
                return Position.Invalid;

            return new Position(location.Line, location.Column);
        }

        /// <summary>
        ///     Get the condition (if any) declared on the element or one of its ancestors.
        /// </summary>
        /// <param name="projectElement">
        ///     The element.
        /// </param>
        /// <returns>
        ///     The condition, or an empty string if no condition is present on the element or one of its ancestors.
        /// </returns>
        public static string FindCondition(this ProjectElement projectElement)
        {
            if (projectElement == null)
                throw new ArgumentNullException(nameof(projectElement));

            ProjectElement currentElement = projectElement;
            while (currentElement != null)
            {
                if (!String.IsNullOrWhiteSpace(currentElement.Condition))
                    return currentElement.Condition;

                currentElement = currentElement.Parent;
            }

            return String.Empty;
        }
    }
}
