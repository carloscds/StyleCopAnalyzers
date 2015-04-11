﻿namespace StyleCop.Analyzers.Test.LayoutRules
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.LayoutRules;
    using TestHelper;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="SA1503CurlyBracketsMustNotBeOmitted"/>.
    /// </summary>
    public class SA1503UnitTests : CodeFixVerifier
    {
        /// <summary>
        /// The statements that will be used in the theory test cases.
        /// </summary>
        public static IEnumerable<object[]> TestStatements
        {
            get
            {
                yield return new[] { "if (i == 0)" };
                yield return new[] { "while (i == 0)" };
                yield return new[] { "for (var j = 0; j < i; j++)" };
                yield return new[] { "foreach (var j in new[] { 1, 2, 3 })" };
            }
        }

        /// <summary>
        /// Verifies that the analyzer will properly handle an empty source.
        /// </summary>
        [Fact]
        public async Task TestEmptySource()
        {
            var testCode = string.Empty;
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        /// <summary>
        /// Verifies that a statement followed by a block without curly braces will produce a warning.
        /// </summary>
        [Theory]
        [MemberData("TestStatements")]
        public async Task TestStatementWithoutCurlyBrackets(string statementText)
        {
            var expected = this.CSharpDiagnostic().WithLocation(7, 13);
            await this.VerifyCSharpDiagnosticAsync(this.GenerateTestStatement(statementText), expected, CancellationToken.None);
        }

        /// <summary>
        /// Verifies that a statement followed by a block with curly braces will produce no diagnostics results.
        /// </summary>
        [Theory]
        [MemberData("TestStatements")]
        public async Task TestStatementWithCurlyBrackets(string statementText)
        {
            await this.VerifyCSharpDiagnosticAsync(this.GenerateFixedTestStatement(statementText), EmptyDiagnosticResults, CancellationToken.None);
        }

        /// <summary>
        /// Verifies that an if / else statement followed by a block without curly braces will produce a warning.
        /// </summary>
        [Fact]
        public async Task TestIfElseStatementWithoutCurlyBrackets()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
            Debug.Assert(true);
        else
            Debug.Assert(false);
    }
}";

            var expected = new[]
            {
                this.CSharpDiagnostic().WithLocation(7, 13),
                this.CSharpDiagnostic().WithLocation(9, 13)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None);
        }

        /// <summary>
        /// Verifies that an if statement followed by a block with curly braces will produce no diagnostics results.
        /// </summary>
        [Fact]
        public async Task TestIfElseStatementWithCurlyBrackets()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else
        {
            Debug.Assert(false);
        }
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        /// <summary>
        /// Verifies that an if statement followed by a else if, followed by an else statement, all blocks with curly braces will produce no diagnostics results.
        /// </summary>
        [Fact]
        public async Task TestIfElseIfElseStatementWithCurlyBrackets()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else if (i == 1)
        {
            Debug.Assert(false);
        }
        else
        {
            Debug.Assert(true);
        }
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        /// <summary>
        /// Verifies that nested if statements followed by a block without curly braces will produce warnings.
        /// </summary>
        [Fact]
        public async Task TestMultipleIfStatementsWithoutCurlyBrackets()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0) if (i == 0) Debug.Assert(true);
    }
}";

            var expected = new[]
            {
                this.CSharpDiagnostic().WithLocation(6, 21),
                this.CSharpDiagnostic().WithLocation(6, 33)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None);
        }


        /// <summary>
        /// Verifies that the codefix provider will work properly for a statement.
        /// </summary>
        [Theory, MemberData("TestStatements")]
        private async Task TestCodeFixForStatement(string statementText)
        {
            await this.VerifyCSharpFixAsync(this.GenerateTestStatement(statementText), this.GenerateFixedTestStatement(statementText));
        }

        /// <summary>
        /// Verifies that the codefix provider will work properly for an if .. else statement.
        /// </summary>
        [Fact]
        public async Task TestCodeFixProviderForIfElseStatement()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else
            Debug.Assert(false);
    }
}";

            var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else
        {
            Debug.Assert(false);
        }
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedTestCode);
        }

        /// <summary>
        /// Verifies that the codefix provider will properly handle alternate indentations.
        /// </summary>
        [Fact(Skip = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/660")]
        public async Task TestCodeFixProviderWithAlternateIndentation()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
 public void Bar(int i)
 {
  if (i == 0)
   Debug.Assert(true);
 }
}";

            var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
 public void Bar(int i)
 {
  if (i == 0)
  {
   Debug.Assert(true);
  }
 }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedTestCode);
        }

        /// <summary>
        /// Verifies that the codefix provider will work properly handle non-whitespace trivia.
        /// </summary>
        [Fact]
        public async Task TestCodeFixProviderWithNonWhitespaceTrivia()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
#pragma warning restore
        if (i == 0)
            Debug.Assert(true);
    }
}";

            var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
#pragma warning restore
        if (i == 0)
        {
            Debug.Assert(true);
        }
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedTestCode);
        }

        /// <summary>
        /// Verifies that the codefix provider will work properly handle multiple cases of missing brackets.
        /// </summary>
        [Fact]
        public async Task TestCodeFixProviderWithMultipleNestings()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0) if (i == 0) Debug.Assert(true);
    }
}";

            var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            if (i == 0)
            {
                Debug.Assert(true);
            }
        }
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedTestCode);
        }

        /// <summary>
        /// Verifies that the code fix provider will not perform a code fix when statement contains a preprocessor directive.
        /// </summary>
        [Fact]
        public async Task TestCodeFixWithPreprocessorDirectives()
        {
            var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (true)
#if !DEBUG
            Debug.WriteLine(""Foobar"");
#endif
        Debug.WriteLine(""Foobar 2"");
    }
}";

            await this.VerifyCSharpFixAsync(testCode, testCode);
        }

        /// <inheritdoc/>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SA1503CurlyBracketsMustNotBeOmitted();
        }

        /// <inheritdoc/>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SA1503CodeFixProvider();
        }

        private string GenerateTestStatement(string statementText)
        {
            var testCodeFormat = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        #STATEMENT#
            Debug.Assert(true);
    }
}";
            return testCodeFormat.Replace("#STATEMENT#", statementText);
        }

        private string GenerateFixedTestStatement(string statementText)
        {
            var fixedTestCodeFormat = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        #STATEMENT#
        {
            Debug.Assert(true);
        }
    }
}";
            return fixedTestCodeFormat.Replace("#STATEMENT#", statementText);
        }
    }
}