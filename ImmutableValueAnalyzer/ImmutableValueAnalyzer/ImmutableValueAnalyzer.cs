using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ImmutableValueAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ImmutableValueAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = nameof(ImmutableValueAnalyzer);

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
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ExpressionStatement);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            AnalyzeExpressionStatement(context, context.Node as ExpressionStatementSyntax);
        }

        private void AnalyzeExpressionStatement(
            SyntaxNodeAnalysisContext context,
            ExpressionStatementSyntax expressionStatementSyntax)
        {
            if (expressionStatementSyntax == null)
                return;

            AnalyzeAssignmentExpression(context, expressionStatementSyntax.Expression as AssignmentExpressionSyntax);
        }

        private void AnalyzeAssignmentExpression(
            SyntaxNodeAnalysisContext context,
            AssignmentExpressionSyntax assignmentExpression)
        {
            var expressionKind = assignmentExpression?.Kind();

            if (expressionKind != SyntaxKind.SimpleAssignmentExpression)
                return;

            AnalyzeIdentifier(context, assignmentExpression, assignmentExpression.Left as IdentifierNameSyntax);
        }

        private void AnalyzeIdentifier(
            SyntaxNodeAnalysisContext context,
            AssignmentExpressionSyntax assignmentExpression,
            IdentifierNameSyntax identifier)
        {
            if (identifier == null || ValueIsMutable(identifier))
                return;

            var valueName = GetValueName(identifier);

            ReportDiagnostic(valueName, context, assignmentExpression.GetLocation());
        }

        private static string GetValueName(IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        private bool ValueIsMutable(IdentifierNameSyntax identifier)
        {
            var valueName = GetValueName(identifier);

            return valueName.StartsWith("_");
        }

        private void ReportDiagnostic(string valueName, SyntaxNodeAnalysisContext context, Location location)
        {
            var diagnostic = Diagnostic.Create(Rule, location, valueName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}