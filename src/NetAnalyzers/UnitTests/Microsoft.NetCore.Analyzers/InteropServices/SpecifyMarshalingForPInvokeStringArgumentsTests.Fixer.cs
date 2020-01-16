﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using System.Threading.Tasks;
using Xunit;
using VerifyCS = TestUtils::Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PInvokeDiagnosticAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.InteropServices.CSharpSpecifyMarshalingForPInvokeStringArgumentsFixer>;
using VerifyVB = TestUtils::Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PInvokeDiagnosticAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.InteropServices.BasicSpecifyMarshalingForPInvokeStringArgumentsFixer>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class SpecifyMarshalingForPInvokeStringArgumentsFixerTests
    {
        #region CA2101 Fixer tests 

        [Fact]
        public async Task CA2101FixMarshalAsCSharpTest()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"")]
    private static extern void Foo1([{|CA2101:MarshalAs(UnmanagedType.LPStr)|}] string s, [{|CA2101:MarshalAs(UnmanagedType.LPStr)|}] StringBuilder t);

    [DllImport(""user32.dll"")]
    private static extern void Foo2([{|CA2101:MarshalAs((short)0)|}] string s);
}
", @"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"")]
    private static extern void Foo1([MarshalAs(UnmanagedType.LPWStr)] string s, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder t);

    [DllImport(""user32.dll"")]
    private static extern void Foo2([MarshalAs(UnmanagedType.LPWStr)] string s);
}
");
        }

        [Fact]
        public async Task CA2101FixMarshalAsBasicTest()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"")>
    Private Shared Sub Foo1(<{|CA2101:MarshalAs(UnmanagedType.LPStr)|}> s As String, <{|CA2101:MarshalAs(UnmanagedType.LPStr)|}> t As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo2(<{|CA2101:MarshalAs(CShort(0))|}> s As String)
    End Sub

    Private Declare Sub Foo3 Lib ""user32.dll"" (<{|CA2101:MarshalAs(UnmanagedType.LPStr)|}> s As String)
End Class
", @"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"")>
    Private Shared Sub Foo1(<MarshalAs(UnmanagedType.LPWStr)> s As String, <MarshalAs(UnmanagedType.LPWStr)> t As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo2(<MarshalAs(UnmanagedType.LPWStr)> s As String)
    End Sub

    Private Declare Sub Foo3 Lib ""user32.dll"" (<MarshalAs(UnmanagedType.LPWStr)> s As String)
End Class
");
        }

        [Fact]
        public async Task CA2101FixCharSetCSharpTest()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [{|CA2101:DllImport(""user32.dll"")|}]
    private static extern void Foo1(string s);

    [{|CA2101:DllImport(""user32.dll"", CharSet = CharSet.Ansi)|}]
    private static extern void Foo2(string s);
}
", @"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern void Foo1(string s);

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern void Foo2(string s);
}
");
        }

        [Fact]
        public async Task CA2101FixCharSetBasicTest()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <{|CA2101:DllImport(""user32.dll"")|}>
    Private Shared Sub Foo1(s As String)
    End Sub

    <{|CA2101:DllImport(""user32.dll"", CharSet:=CharSet.Ansi)|}>
    Private Shared Sub Foo2(s As String)
    End Sub
End Class
", @"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"", CharSet:=CharSet.Unicode)>
    Private Shared Sub Foo1(s As String)
    End Sub

    <DllImport(""user32.dll"", CharSet:=CharSet.Unicode)>
    Private Shared Sub Foo2(s As String)
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA2101FixDeclareBasicTest()
        {
            await VerifyVB.VerifyCodeFixAsync(@"
Imports System.Text

Class C
    Private Declare Sub {|CA2101:Foo1|} Lib ""user32.dll"" (s As String)
    Private Declare Ansi Sub {|CA2101:Foo2|} Lib ""user32.dll"" (s As StringBuilder)
    Private Declare Function {|CA2101:Foo3|} Lib ""user32.dll"" () As String
End Class
", @"
Imports System.Text

Class C
    Private Declare Unicode Sub Foo1 Lib ""user32.dll"" (s As String)
    Private Declare Unicode Sub Foo2 Lib ""user32.dll"" (s As StringBuilder)
    Private Declare Unicode Function Foo3 Lib ""user32.dll"" () As String
End Class
");
        }

        #endregion
    }
}
