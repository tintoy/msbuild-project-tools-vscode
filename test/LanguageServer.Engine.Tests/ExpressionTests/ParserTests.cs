using System;
using Xunit.Abstractions;

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    /// <summary>
    ///     The base class for parser test suites.
    /// </summary>
    public abstract class ParserTests
    {
        /// <summary>
        ///     Create a new parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        protected ParserTests(ITestOutputHelper testOutput)
        {
            if (testOutput == null)
                throw new ArgumentNullException(nameof(testOutput));

            TestOutput = testOutput;
        }

        /// <summary>
        ///     Output for the current test.
        /// </summary>
        protected ITestOutputHelper TestOutput { get; }
    }
}
