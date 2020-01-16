﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias TestUtils;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = TestUtils::Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseObsoleteKDFAlgorithm,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseObsoleteKDFAlgorithmTests
    {
        [Fact]
        public async Task TestNormalMethodOfPasswordDeriveBytesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(PasswordDeriveBytes passwordDeriveBytes)
    {
        passwordDeriveBytes.GetBytes(1);
    }
}",
            GetCSharpResultAt(9, 9, "PasswordDeriveBytes", "GetBytes"));
        }

        [Fact]
        public async Task TestCryptDeriveKeyOfClassDerivedFromPasswordDeriveBytesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class DerivedClass : PasswordDeriveBytes
{
    public DerivedClass(string password, byte[] salt) : base(password, salt)
    {
    }
}

class TestClass
{
    public void TestMethod(DerivedClass derivedClass, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        derivedClass.CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(16, 9, "PasswordDeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public async Task TestCryptDeriveKeyOfRfc2898DeriveBytesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(Rfc2898DeriveBytes rfc2898DeriveBytes, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        rfc2898DeriveBytes.CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(9, 9, "Rfc2898DeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public async Task TestCryptDeriveKeyOfClassDerivedFromRfc2898DeriveBytesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class DerivedClass : Rfc2898DeriveBytes
{
    public DerivedClass(string password, byte[] salt) : base(password, salt)
    {
    }
}

class TestClass
{
    public void TestMethod(DerivedClass derivedClass, string algname, string alghashname, int keySize, byte[] rgbIV)
    {
        derivedClass.CryptDeriveKey(algname, alghashname, keySize, rgbIV);
    }
}",
            GetCSharpResultAt(16, 9, "Rfc2898DeriveBytes", "CryptDeriveKey"));
        }

        [Fact]
        public async Task TestNormalMethodOfRfc2898DeriveBytesNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(Rfc2898DeriveBytes rfc2898DeriveBytes)
    {
        rfc2898DeriveBytes.GetBytes(1);
    }
}");
        }

        [Fact]
        public async Task TestConstructorOfRfc2898DeriveBytesNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        new Rfc2898DeriveBytes(password, salt);
    }
}");
        }

        [Fact]
        public async Task TestConstructorOfPasswordDeriveBytesNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class TestClass
{
    public void TestMethod(string password, byte[] salt)
    {
        new PasswordDeriveBytes(password, salt);
    }
}");
        }

        [Fact]
        public async Task TestGetBytesOfClassDerivedFromPasswordDeriveBytesNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Security.Cryptography;

class DerivedClass : PasswordDeriveBytes
{
    public DerivedClass(string password, byte[] salt) : base(password, salt)
    {
    }

    public override byte[] GetBytes (int cb)
    {
        return null;
    }
}

class TestClass
{
    public void TestMethod(string password, byte[] salt, int cb)
    {
        new DerivedClass(password, salt).GetBytes(cb);
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
