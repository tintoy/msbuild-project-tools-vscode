using System;
using Xunit.Abstractions;

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    /// <summary>
    ///     The base class for parser test suites.
    /// </summary>
    public abstract class ParserTests
        : TestBase
    {
        /// <summary>
        ///     Create a new parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        protected ParserTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }
    }
}
