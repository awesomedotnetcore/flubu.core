using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace FlubuClore.Analyzer.Tests
{
    [TestClass]
    public class TargetParameterAnalyzerUnitTests : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void CorrectTargetDefinititionTest()
        {
            var test = @"
  using System;
using System.IO;
using FlubuCore.Context;
using FlubuCore.Context.FluentInterface;
using FlubuCore.Context.FluentInterface.Interfaces;
using FlubuCore.Scripting;
using FlubuCore.Targeting;
using Moq;

namespace FlubuCore.WebApi.Tests
{
    public class SimpleScript : DefaultBuildScript
    {
        protected override void ConfigureBuildProperties(IBuildPropertiesContext context)
        {
        }

        protected override void ConfigureTargets(ITaskContext session)
        {
        }

        [Target(\""Test\"")]
        public void SuccesfullTarget(ITarget target, string fileName)
        {
        }
     }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void WrongFirstParameterTest()
        {
            var test = @"
  using System;
using System.IO;
using FlubuCore.Context;
using FlubuCore.Context.FluentInterface;
using FlubuCore.Context.FluentInterface.Interfaces;
using FlubuCore.Scripting;
using FlubuCore.Targeting;
using Moq;

namespace FlubuCore.WebApi.Tests
{
    public class SimpleScript : DefaultBuildScript
    {
        protected override void ConfigureBuildProperties(IBuildPropertiesContext context)
        {
        }

        protected override void ConfigureTargets(ITaskContext session)
        {
        }

        [Target(\""Test\"")]
        public void SuccesfullTarget(string fileName)
        {
        }
     }
}";
            var expected = new DiagnosticResult
            {
                Id = "FlubuCore_TargetParameterAnalyzer",
                Message = String.Format("First parameter in method '{0}' must be of type ITarget.", "SuccesfullTarget"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 24, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void NoFirstParameterTest()
        {
            var test = @"
  using System;
using System.IO;
using FlubuCore.Context;
using FlubuCore.Context.FluentInterface;
using FlubuCore.Context.FluentInterface.Interfaces;
using FlubuCore.Scripting;
using FlubuCore.Targeting;
using Moq;

namespace FlubuCore.WebApi.Tests
{
    public class SimpleScript : DefaultBuildScript
    {
        protected override void ConfigureBuildProperties(IBuildPropertiesContext context)
        {
        }

        protected override void ConfigureTargets(ITaskContext session)
        {
        }

        [Target(\""Test\"")]
        public void SuccesfullTarget()
        {
        }
     }
}";
            var expected = new DiagnosticResult
            {
                Id = "FlubuCore_TargetParameterAnalyzer",
                Message = String.Format("First parameter in method '{0}' must be of type ITarget.", "SuccesfullTarget"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 24, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new FlubuCloreAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TargetParameterAnalyzer();
        }
    }
}