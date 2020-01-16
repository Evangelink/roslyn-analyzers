// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = TestUtils::Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetFramework.Analyzers.DoNotUseInsecureDtdProcessingAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = TestUtils::Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.NetFramework.Analyzers.DoNotUseInsecureDtdProcessingAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDtdProcessingAnalyzerTests
    {
        private static DiagnosticResult GetCA3075SchemaReadCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic().WithLocation(line, column).WithArguments(string.Format(CultureInfo.CurrentCulture, MicrosoftNetFrameworkAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage, "Read"));

        private static DiagnosticResult GetCA3075SchemaReadBasicResultAt(int line, int column)
            => VerifyCS.Diagnostic().WithLocation(line, column).WithArguments(string.Format(CultureInfo.CurrentCulture, MicrosoftNetFrameworkAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage, "Read"));

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Xml.Schema;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            TextReader tr = new StreamReader(path);
            XmlSchema schema = XmlSchema.Read(tr, null);
        }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(12, 32)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.IO
Imports System.Xml.Schema

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim tr As TextReader = New StreamReader(path)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        End Sub
    End Class
End Namespace",
                GetCA3075SchemaReadBasicResultAt(9, 39)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderInGetShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Xml.Schema;

class TestClass
{
    public XmlSchema Test
    {
        get
        {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlSchema schema = XmlSchema.Read(tr, null);
            return schema;
        }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(13, 32)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.IO
Imports System.Xml.Schema

Class TestClass
    Public ReadOnly Property Test() As XmlSchema
        Get
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
            Return schema
        End Get
    End Property
End Class",
                GetCA3075SchemaReadBasicResultAt(10, 39)
            );
        }

        [Fact]
        public async Task UseUseXmlSchemaReadWithoutXmlTextReaderInSetShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Data;
using System.IO;
using System.Xml.Schema;

class TestClass
{
    XmlSchema privateDoc;
    public XmlSchema GetDoc
    {
        set
        {
            if (value == null)
            {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
                privateDoc = schema;
            }
            else
                privateDoc = value;
        }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(17, 36)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Data
Imports System.IO
Imports System.Xml.Schema

Class TestClass
    Private privateDoc As XmlSchema
    Public WriteOnly Property GetDoc() As XmlSchema
        Set
            If value Is Nothing Then
                Dim src = """"
                Dim tr As TextReader = New StreamReader(src)
                Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
                privateDoc = schema
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075SchemaReadBasicResultAt(13, 43)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderInTryBlockShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Data;
using System.IO;
using System.Xml.Schema;
class TestClass
{
    private void TestMethod()
    {
        try
        {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlSchema schema = XmlSchema.Read(tr, null);
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(14, 32)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(11, 39)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderInCatchBlockShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Data;
    using System.IO;
    using System.Xml.Schema;
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception)
            {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
            }
            finally { }
        }
    }",
                GetCA3075SchemaReadCSharpResultAt(15, 36)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(12, 39)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderInFinallyBlockShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Data;
    using System.IO;
    using System.Xml.Schema;
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { throw; }
            finally
            {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
            }
        }
    }",
                GetCA3075SchemaReadCSharpResultAt(16, 36)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        End Try
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(14, 39)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderInAsyncAwaitShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Xml.Schema;
    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
            });
        }

        private async void TestMethod2()
        {
            await TestMethod();
        }
    }",
                GetCA3075SchemaReadCSharpResultAt(13, 36)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Threading.Tasks
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Async Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim tr As TextReader = New StreamReader(src)
        Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)

End Function)
    End Function

    Private Async Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(11, 35)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithoutXmlTextReaderInDelegateShouldGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Data;
using System.IO;
using System.Xml.Schema;
    class TestClass
    {
        delegate void Del();

        Del d = delegate () {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlSchema schema = XmlSchema.Read(tr, null);
        };
    }",
                GetCA3075SchemaReadCSharpResultAt(12, 32)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim tr As TextReader = New StreamReader(src)
    Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)

End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(11, 31)
            );
        }

        [Fact]
        public async Task UseXmlSchemaReadWithXmlTextReaderShouldNotGenerateDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Xml;
using System.Xml.Schema;

namespace TestNamespace
{
    public class UseXmlReaderForSchemaRead
    {
        public void TestMethod19(XmlTextReader reader)
        {
            XmlSchema schema = XmlSchema.Read(reader, null);
        }
    }
}"
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Xml
Imports System.Xml.Schema

Namespace TestNamespace
    Public Class UseXmlReaderForSchemaRead
        Public Sub TestMethod19(reader As XmlTextReader)
            Dim schema As XmlSchema = XmlSchema.Read(reader, Nothing)
        End Sub
    End Class
End Namespace");
        }
    }
}
