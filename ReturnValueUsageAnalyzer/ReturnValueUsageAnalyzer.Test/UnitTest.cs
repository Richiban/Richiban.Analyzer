using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ReturnValueUsageAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void EmpytSourceDoesNotTriggerDiagnostic()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void UnusedStringReturnTypeTriggersDiagnostic()
        {
            var test = @"
using System;

namespace ConsoleApplication1
{
    class TypeName
    {
        public void A()
        {
            B();
        }

        public string B()
        {
            return string.Empty;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = nameof(Richiban.ReturnUsageAnalyzer.ReturnValueUsageAnalyzer),
                Message = "The returned value of type 'string' is not used.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line: 10, column: 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void UnitDoesNotTriggerDiagnostic()
        {
            var test = @"
using System;

namespace ConsoleApplication1
{
    class TypeName
    {
        public void A()
        {
            B();
        }

        public Unit B()
        {
            return new Unit();
        }
    }

    class Unit
    {
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void DynamicDoesNotTriggerDiagnostic()
        {
            var test = @"
using System;

namespace ConsoleApplication1
{
    class TypeName
    {
        public void A()
        {
            B();
        }

        public dynamic B()
        {
            return new object();
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void CompilerErrorDoesNotTriggerDiagnostic()
        {
            var test = @"
using System;

namespace ConsoleApplication1
{
    class TypeName
    {
        public void A()
        {
            B();
        }
    }

    class Unit
    {
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new Richiban.ReturnUsageAnalyzer.ReturnValueUsageAnalyzer();
    }
}