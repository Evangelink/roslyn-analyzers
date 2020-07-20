// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using Roslyn.Diagnostics.Analyzers;

namespace Roslyn.Diagnostics.CSharp.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public sealed class CSharpAvoidOptSuffixForNullableEnableCodeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSharpAvoidOptSuffixForNullableEnableCode.Rule.Id);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var title = RoslynDiagnosticsAnalyzersResources.AvoidOptSuffixForNullableEnableCodeCodeFixTitle;

            foreach (var diagnostic in context.Diagnostics)
            {
                var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                var variable = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                if (variable == null)
                {
                    continue;
                }

                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                var variableSymbol = semanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
                if (variableSymbol == null || variableSymbol.Name.Length <= CSharpAvoidOptSuffixForNullableEnableCode.OptSuffix.Length)
                {
                    continue;
                }

                var newName = variableSymbol.Name.Substring(0, variableSymbol.Name.Length - CSharpAvoidOptSuffixForNullableEnableCode.OptSuffix.Length);

                var lookupSymbolsPosition = diagnostic.Properties.TryGetValue(CSharpAvoidOptSuffixForNullableEnableCode.MemberBodySpanEnd, out string memberBodySpanEndValue)
                    && int.TryParse(memberBodySpanEndValue, out int memberBodySpanEnd)
                    ? memberBodySpanEnd
                    : diagnostic.Location.SourceSpan.End;

                // Check there is no conflicting symbol before we register the codefix
                if (semanticModel.LookupSymbols(lookupSymbolsPosition, name: newName).IsEmpty)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title,
                            cancellationToken => RemoveOptSuffixOnVariableAsync(context.Document, variableSymbol, newName, cancellationToken),
                            equivalenceKey: title),
                        diagnostic);
                }
            }
        }

        private static async Task<Solution> RemoveOptSuffixOnVariableAsync(Document document, ISymbol variableSymbol, string newName, CancellationToken cancellationToken)
            => await Renamer.RenameSymbolAsync(document.Project.Solution, variableSymbol, newName, document.Project.Solution.Options, cancellationToken)
                .ConfigureAwait(false);
    }
}
