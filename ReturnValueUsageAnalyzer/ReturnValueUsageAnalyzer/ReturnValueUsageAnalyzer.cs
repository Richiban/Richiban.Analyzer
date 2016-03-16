using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReturnValueUsageAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReturnValueUsageAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = nameof(ReturnValueUsageAnalyzer);

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle),
            Resources.ResourceManager,
            typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerMessageFormat),
                Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerDescription),
                Resources.ResourceManager,
                typeof (Resources));

        private const string Category = "Functional";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ExpressionStatement);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var expressionStatementSyntax = context.Node as ExpressionStatementSyntax;

            if (expressionStatementSyntax == null)
                return;

            var typeInfo = context.SemanticModel.GetTypeInfo(expressionStatementSyntax.Expression);

            if (typeInfo.Type.SpecialType != SpecialType.System_Void)
            {
                ReportDiagnostic(typeInfo, context, expressionStatementSyntax.GetLocation());
            }
        }

        private void ReportDiagnostic(TypeInfo typeInfo, SyntaxNodeAnalysisContext context, Location location)
        {
            var diagnostic = Diagnostic.Create(Rule, location, typeInfo.Type);

            context.ReportDiagnostic(diagnostic);
        }
    }
}