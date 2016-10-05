using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Richiban.ReturnUsageAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReturnValueUsageAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = nameof(ReturnValueUsageAnalyzer);

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerMessageFormat),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerDescription),
                Resources.ResourceManager,
                typeof(Resources));

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

            var expression = expressionStatementSyntax.Expression;

            if (expression.Kind() == SyntaxKind.SimpleAssignmentExpression)
                return;

            var typeInfo = context.SemanticModel.GetTypeInfo(expression);

            if (ShouldIgnoreExpressionType(typeInfo))
                return;

            ReportDiagnostic(typeInfo, context, expressionStatementSyntax.GetLocation());
        }

        private static bool ShouldIgnoreExpressionType(TypeInfo typeInfo)
        {
            var rules = new List<Func<ITypeSymbol, bool>>
            {
                type => "Unit".Equals(type?.Name, StringComparison.OrdinalIgnoreCase),
                type => type?.Name == null,
                type => type?.TypeKind == TypeKind.Error,
                type => type?.TypeKind == TypeKind.Dynamic,
                type => type?.SpecialType == SpecialType.System_Void
            };

            return rules.Any(rule => rule(typeInfo.Type));
        }

        private void ReportDiagnostic(TypeInfo typeInfo, SyntaxNodeAnalysisContext context, Location location)
        {
            var diagnostic = Diagnostic.Create(Rule, location, typeInfo.Type);

            context.ReportDiagnostic(diagnostic);
        }
    }
}