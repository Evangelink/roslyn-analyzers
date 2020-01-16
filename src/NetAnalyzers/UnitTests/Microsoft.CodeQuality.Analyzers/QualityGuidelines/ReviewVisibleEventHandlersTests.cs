// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using TestUtils::Test.Utilities;
using VerifyCS = TestUtils::Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpReviewVisibleEventHandlersAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpReviewVisibleEventHandlersFixer>;
using VerifyVB = TestUtils::Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines.BasicReviewVisibleEventHandlersAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines.BasicReviewVisibleEventHandlersFixer>;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public class ReviewVisibleEventHandlersTests
    {
    }
}