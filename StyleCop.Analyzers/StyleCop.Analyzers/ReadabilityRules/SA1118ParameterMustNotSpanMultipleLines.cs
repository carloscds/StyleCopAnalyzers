﻿namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// A parameter to a C# method or indexer, other than the first parameter, spans across multiple lines.
    /// </summary>
    /// <remarks>
    /// <para>To prevent method calls from becoming excessively complicated and unreadable, only the first parameter to
    /// a method or indexer call is allowed to span across multiple lines. The exception is an anonymous method passed
    /// as a parameter, which is always allowed to span multiple lines. A violation of this rule occurs whenever a
    /// parameter other than the first parameter spans across multiple lines, and the parameter does not contain an
    /// anonymous method.</para>
    ///
    /// <para>For example, the following code would violate this rule, since the second parameter spans across multiple
    /// lines:</para>
    ///
    /// <code language="csharp">
    /// return JoinStrings(
    ///     "John",
    ///     "Smith" +
    ///     " Doe");
    /// </code>
    ///
    /// <para>When parameters other than the first parameter span across multiple lines, it can be difficult to tell how
    /// many parameters are passed to the method. In general, the code becomes difficult to read.</para>
    ///
    /// <para>To fix the example above, ensure that the parameters after the first parameter do not span across multiple
    /// lines. If this will cause a parameter to be excessively long, store the value of the parameter within a
    /// temporary variable. For example:</para>
    ///
    /// <code language="csharp">
    /// string last = "Smith" +
    ///     " Doe";
    ///
    /// return JoinStrings(
    ///     "John",
    ///     last);
    /// </code>
    ///
    /// <para>In some cases, this will allow the method to be written even more concisely, such as:</para>
    ///
    /// <code language="csharp">
    /// return JoinStrings("John", last);
    /// </code>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1118ParameterMustNotSpanMultipleLines : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1118ParameterMustNotSpanMultipleLines"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1118";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReadabilityResources.SA1118Title), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReadabilityResources.SA1118MessageFormat), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string Category = "StyleCop.CSharp.ReadabilityRules";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReadabilityResources.SA1118Description), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string HelpLink = "http://www.stylecop.com/docs/SA1118.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        private static readonly SyntaxKind[] ArgumentExceptionSyntaxKinds =
        {
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.SimpleLambdaExpression
        };

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleArgumentList, SyntaxKind.ArgumentList, SyntaxKind.BracketedArgumentList);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleAttributeArgumentList, SyntaxKind.AttributeArgumentList);
        }

        private void HandleAttributeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var attributeArgumentList = (AttributeArgumentListSyntax)context.Node;

            for (int i = 1; i < attributeArgumentList.Arguments.Count; i++)
            {
                var argument = attributeArgumentList.Arguments[i];
                if (this.CheckIfArgumentIsMultiline(argument))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
                }
            }
        }

        private void HandleArgumentList(SyntaxNodeAnalysisContext context)
        {
            var argumentListSyntax = (BaseArgumentListSyntax)context.Node;

            for (int i = 1; i < argumentListSyntax.Arguments.Count; i++)
            {
                var argument = argumentListSyntax.Arguments[i];
                if (this.CheckIfArgumentIsMultiline(argument)
                    && !this.IsArgumentOnExceptionList(argument.Expression))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
                }
            }
        }

        private bool CheckIfArgumentIsMultiline(CSharpSyntaxNode argument)
        {
            var lineSpan = argument.GetLocation().GetLineSpan();
            return lineSpan.EndLinePosition.Line > lineSpan.StartLinePosition.Line;
        }

        private bool IsArgumentOnExceptionList(ExpressionSyntax argumentExpresson)
        {
            return argumentExpresson != null
                && ArgumentExceptionSyntaxKinds.Any(argumentExpresson.IsKind);
        }
    }
}
