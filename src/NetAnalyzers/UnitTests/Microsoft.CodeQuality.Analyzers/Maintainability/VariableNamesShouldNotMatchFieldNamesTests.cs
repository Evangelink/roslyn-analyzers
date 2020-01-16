// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using TestUtils::Test.Utilities;
using VerifyCS = TestUtils::Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpVariableNamesShouldNotMatchFieldNamesAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpVariableNamesShouldNotMatchFieldNamesFixer>;
using VerifyVB = TestUtils::Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability.BasicVariableNamesShouldNotMatchFieldNamesAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability.BasicVariableNamesShouldNotMatchFieldNamesFixer>;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class VariableNamesShouldNotMatchFieldNamesTests
    {
    }
}