﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = TestUtils::Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseXslTransform,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseXslTransformTests
    {
        [Fact]
        public async Task TestConstructXslTransformDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Xml.Xsl;

class TestClass
{
    public void TestMethod()
    {
        new XslTransform();
    }
}",
            GetCSharpResultAt(9, 9));
        }

        [Fact]
        public async Task TestConstructNormalClassNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Xml.Xsl;

class TestClass
{
    public void TestMethod()
    {
        new TestClass();
    }
}");
        }

        [Fact]
        public async Task TestInvokeMethodOfXslTransformNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Xml.Xsl;

class TestClass
{
    public void TestMethod(XslTransform xslTransform)
    {
        xslTransform.Load(""url"");
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);
    }
}
