﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using System.Threading.Tasks;
using Xunit;
using VerifyCS = TestUtils::Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidUnsealedAttributesAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidUnsealedAttributesFixer>;
using VerifyVB = TestUtils::Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidUnsealedAttributesAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.AvoidUnsealedAttributesFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class AvoidUnsealedAttributeFixerTests
    {
        #region CodeFix Tests

        [Fact]
        public async Task CA1813CSharpCodeFixProviderTestFired()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;

public class [|AttributeClass|] : Attribute
{
}", @"
using System;

public sealed class AttributeClass : Attribute
{
}");
        }

        [Fact]
        public async Task CA1813VisualBasicCodeFixProviderTestFired()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System

Public Class [|AttributeClass|]
    Inherits Attribute
End Class", @"
Imports System

Public NotInheritable Class AttributeClass
    Inherits Attribute
End Class");
        }

        #endregion
    }
}
