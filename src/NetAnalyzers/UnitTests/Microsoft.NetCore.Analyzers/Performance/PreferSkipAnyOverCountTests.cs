// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UseCountProperlyAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpPreferIsEmptyOverCountFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UseCountProperlyAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicPreferIsEmptyOverCountFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class PreferSkipAnyOverCountTests
    {
        [Fact]
        public async Task CountGreaterThanTwo_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class C
{
    public void M(IEnumerable<int> list)
    {
        if ({|CA1837:list.Count() > 2|})
        {
        }

        if ({|CA1837:2 < list.Count()|})
        {
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"");
        }

        [Fact]
        public async Task CountGreaterThanOrEqualToTwo_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class C
{
    public void M(IEnumerable<int> list)
    {
        if ({|CA1837:list.Count() >= 2|})
        {
        }

        if ({|CA1837:2 <= list.Count()|})
        {
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"");
        }

        [Fact]
        public async Task CountEqualToTwo_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class C
{
    public void M(IEnumerable<int> list)
    {
        if ({|CA1837:list.Count() == 2|})
        {
        }

        if ({|CA1837:2 == list.Count()|})
        {
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"");
        }

        [Fact]
        public async Task CountLessThanOrEqualToTwo_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class C
{
    public void M(IEnumerable<int> list)
    {
        if ({|CA1837:list.Count() <= 2|})
        {
        }

        if ({|CA1837:2 >= list.Count()|})
        {
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"");
        }

        [Fact]
        public async Task CountLessThanTwo_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class C
{
    public void M(IEnumerable<int> list)
    {
        if ({|CA1837:list.Count() < 2|})
        {
        }

        if ({|CA1837:2 > list.Count()|})
        {
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"");
        }

        [Fact]
        public async Task Count_WhenConstantValueGreaterThanInt_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class C
{
    public void M(IEnumerable<int> list)
    {
        if (list.Count() < uint.MaxValue)
        {
        }

        if (uint.MaxValue > list.Count())
        {
        }
    }
}");
        }
    }
}
