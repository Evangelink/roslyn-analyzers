// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Testing;
using TestUtils::Test.Utilities;
using Xunit;
using VerifyCS = TestUtils::Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.MarkAssembliesWithAttributesDiagnosticAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpMarkAssembliesWithAssemblyVersionFixer>;
using VerifyVB = TestUtils::Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.MarkAssembliesWithAttributesDiagnosticAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines.BasicMarkAssembliesWithAssemblyVersionFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class MarkAssembliesWithAssemblyVersionAttributeTests
    {
        [Fact]
        public async Task CA1016BasicTestWithNoComplianceAttribute()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"
imports System.IO
imports System.Reflection
imports System

< Assembly: CLSCompliant(true)>
    class Program
    
        Sub Main
        End Sub
    End class
",
                s_diagnostic);
        }

        [Fact]
        public async Task CA1016CSharpTestWithVersionAttributeNotFromBCL()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
using System;
[assembly:System.CLSCompliantAttribute(true)]
[assembly:AssemblyVersion(""1.2.3.4"")]
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
class AssemblyVersionAttribute : Attribute {
    public AssemblyVersionAttribute(string s) {}
}
",
                s_diagnostic);
        }

        [Fact]
        public async Task CA1016CSharpTestWithNoVersionAttribute()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
[assembly:System.CLSCompliantAttribute(true)]

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
",
                s_diagnostic);
        }

        [Fact]
        public async Task CA1016CSharpTestWithVersionAttribute()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
using System.Reflection;
[assembly:AssemblyVersionAttribute(""1.2.3.4"")]
[assembly:System.CLSCompliantAttribute(true)]

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
");
        }

        [Fact]
        public async Task CA1016CSharpTestWithTwoFilesWithAttribute()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
@"
[assembly:System.CLSCompliantAttribute(true)]

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
",
@"
using System.Reflection;
[assembly: AssemblyVersionAttribute(""1.2.3.4"")]
"
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1016CSharpTestWithVersionAttributeTruncated()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
using System.Reflection;
[assembly:AssemblyVersion(""1.2.3.4"")]
[assembly:System.CLSCompliantAttribute(true)]
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
");
        }

        [Fact]
        public async Task CA1016CSharpTestWithVersionAttributeFullyQualified()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
[assembly:System.CLSCompliantAttribute(true)]

[assembly:System.Reflection.AssemblyVersion(""1.2.3.4"")]
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
");
        }

        [Fact, WorkItem(2143, "https://github.com/dotnet/roslyn-analyzers/issues/2143")]
        public async Task CA1016CSharpTestWithRazorCompiledItemAttribute()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System;

[assembly:Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute((Type)null, null, null)]

namespace Microsoft.AspNetCore.Razor.Hosting
{
    public class RazorCompiledItemAttribute : Attribute
    {
        public RazorCompiledItemAttribute(Type type, string kind, string identifier)
        {
        }
    }
}

public class C
{
}
");
        }

        private static readonly DiagnosticResult s_diagnostic = new DiagnosticResult(MarkAssembliesWithAttributesDiagnosticAnalyzer.CA1016RuleId, DiagnosticHelpers.DefaultDiagnosticSeverity)
            .WithMessageFormat(MicrosoftCodeQualityAnalyzersResources.MarkAssembliesWithAssemblyVersionMessage);
    }
}
