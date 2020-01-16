// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    internal sealed class SymbolNamesOption : IEquatable<SymbolNamesOption?>
    {
        private const SymbolKind AllKinds = SymbolKind.ErrorType;

        public static readonly SymbolNamesOption Empty = new SymbolNamesOption();

        private readonly ImmutableHashSet<string> _names;
        private readonly ImmutableHashSet<ISymbol> _symbols;
        private readonly ImmutableDictionary<SymbolKind, ImmutableHashSet<string>> _wildcardNamesBySymbolKind;

        private SymbolNamesOption(ImmutableHashSet<string> names, ImmutableHashSet<ISymbol> symbols, ImmutableDictionary<SymbolKind, ImmutableHashSet<string>> wildcardNamesBySymbolKind)
        {
            Debug.Assert(!names.IsEmpty || !symbols.IsEmpty || !wildcardNamesBySymbolKind.IsEmpty);

            _names = names;
            _symbols = symbols;
            _wildcardNamesBySymbolKind = wildcardNamesBySymbolKind;
        }

        private SymbolNamesOption()
        {
            _names = ImmutableHashSet<string>.Empty;
            _symbols = ImmutableHashSet<ISymbol>.Empty;
            _wildcardNamesBySymbolKind = ImmutableDictionary<SymbolKind, ImmutableHashSet<string>>.Empty;
        }

        public static SymbolNamesOption Create(ImmutableArray<string> symbolNames, Compilation compilation, string? optionalPrefix)
        {
            if (symbolNames.IsEmpty)
            {
                return Empty;
            }

            var namesBuilder = PooledHashSet<string>.GetInstance();
            var wildcardNamesBuilder = PooledDictionary<SymbolKind, PooledHashSet<string>>.GetInstance();
            var symbolsBuilder = PooledHashSet<ISymbol>.GetInstance();

            foreach (var name in symbolNames)
            {
                var numberOfWildcards = name.Count(c => c == '*');

                if (numberOfWildcards > 1 ||
                    (numberOfWildcards == 1 && name[name.Length - 1] != '*'))
                {
                    // This is a currently unhandled scenario
                    continue;
                }

                if (numberOfWildcards == 1)
                {
                    Debug.Assert(name[name.Length - 1] == '*');

                    if (name[1] != ':')
                    {
                        if (!wildcardNamesBuilder.ContainsKey(AllKinds))
                        {
                            wildcardNamesBuilder.Add(AllKinds, PooledHashSet<string>.GetInstance());
                        }
                        wildcardNamesBuilder[AllKinds].Add(name.Substring(0, name.Length - 1));
                        continue;
                    }

                    var symbolKind = name[0] switch
                    {
                        'E' => (SymbolKind?)SymbolKind.Event,
                        'F' => SymbolKind.Field,
                        'M' => SymbolKind.Method,
                        'N' => SymbolKind.Namespace,
                        'P' => SymbolKind.Property,
                        'T' => SymbolKind.NamedType,
                        _ => null,
                    };

                    if (symbolKind != null)
                    {
                        if (!wildcardNamesBuilder.ContainsKey(symbolKind.Value))
                        {
                            wildcardNamesBuilder.Add(symbolKind.Value, PooledHashSet<string>.GetInstance());
                        }
                        wildcardNamesBuilder[symbolKind.Value].Add(name.Substring(2, name.Length - 3));
                    }

                    continue;
                }

                if (name.Equals(".ctor", StringComparison.Ordinal) ||
                    name.Equals(".cctor", StringComparison.Ordinal) ||
                    !name.Contains(".") && !name.Contains(":"))
                {
                    namesBuilder.Add(name);
                    continue;
                }

                var nameWithPrefix = (string.IsNullOrEmpty(optionalPrefix) || name.StartsWith(optionalPrefix, StringComparison.Ordinal)) ?
                    name :
                    optionalPrefix + name;

#pragma warning disable CA1307 // Specify StringComparison - https://github.com/dotnet/roslyn-analyzers/issues/1552
                // Documentation comment ID for constructors uses '#ctor', but '#' is a comment start token for editorconfig.
                // We instead search for a '..ctor' in editorconfig and replace it with a '.#ctor' here.
                // Similarly, handle static constructors ".cctor"
                nameWithPrefix = nameWithPrefix.Replace("..ctor", ".#ctor");
                nameWithPrefix = nameWithPrefix.Replace("..cctor", ".#cctor");
#pragma warning restore

                foreach (var symbol in DocumentationCommentId.GetSymbolsForDeclarationId(nameWithPrefix, compilation))
                {
                    if (symbol != null)
                    {
                        if (symbol is INamespaceSymbol namespaceSymbol &&
                            namespaceSymbol.ConstituentNamespaces.Length > 1)
                        {
                            foreach (var constituentNamespace in namespaceSymbol.ConstituentNamespaces)
                            {
                                symbolsBuilder.Add(constituentNamespace);
                            }
                        }

                        symbolsBuilder.Add(symbol);
                    }
                }
            }

            if (namesBuilder.Count == 0 && symbolsBuilder.Count == 0 && wildcardNamesBuilder.Count == 0)
            {
                return Empty;
            }

            return new SymbolNamesOption(namesBuilder.ToImmutableAndFree(), symbolsBuilder.ToImmutableAndFree(),
                wildcardNamesBuilder.ToImmutableDictionaryAndFree(x => x.Key, x => x.Value.ToImmutableAndFree(), wildcardNamesBuilder.Comparer));
        }

        public bool IsEmpty => ReferenceEquals(this, Empty);

        public bool Contains(ISymbol symbol)
            => _symbols.Contains(symbol) || _names.Contains(symbol.Name) || HasAnyWildcardMatch(symbol);

        public override bool Equals(object obj) => Equals(obj as SymbolNamesOption);

        public bool Equals(SymbolNamesOption? other)
            => other != null &&
                _names.SetEquals(other._names) &&
                _symbols.SetEquals(other._symbols) &&
                _wildcardNamesBySymbolKind.Count == other._wildcardNamesBySymbolKind.Count &&
                _wildcardNamesBySymbolKind.Keys.All(key => other._wildcardNamesBySymbolKind.ContainsKey(key) && _wildcardNamesBySymbolKind[key].SetEquals(other._wildcardNamesBySymbolKind[key]));

        public override int GetHashCode()
            => HashUtilities.Combine(HashUtilities.Combine(_names), HashUtilities.Combine(_symbols), HashUtilities.Combine(_wildcardNamesBySymbolKind));

        private bool HasAnyWildcardMatch(ISymbol symbol)
        {
            if (_wildcardNamesBySymbolKind.IsEmpty)
            {
                return false;
            }

            if (symbol.Kind != SymbolKind.Event &&
                symbol.Kind != SymbolKind.Field &&
                symbol.Kind != SymbolKind.Method &&
                symbol.Kind != SymbolKind.NamedType &&
                symbol.Kind != SymbolKind.Namespace &&
                symbol.Kind != SymbolKind.Property)
            {
                return false;
            }

            var symbolFullNameBuilder = new StringBuilder();
            var symbolKindsToCheck = new HashSet<SymbolKind> { symbol.Kind };

            if (MatchesSymbolPart(symbol))
            {
                return true;
            }

            INamedTypeSymbol? currentType = symbol.ContainingType;
            while (currentType != null)
            {
                if (MatchesSymbolPart(currentType))
                {
                    return true;
                }

                symbolKindsToCheck.Add(SymbolKind.NamedType);
                currentType = currentType.ContainingType;
            }

            INamespaceSymbol? currentNamespace = symbol.ContainingNamespace;
            while (currentNamespace != null && !currentNamespace.IsGlobalNamespace)
            {
                if (MatchesSymbolPart(currentNamespace))
                {
                    return true;
                }

                symbolKindsToCheck.Add(SymbolKind.Namespace);
                currentNamespace = currentNamespace.ContainingNamespace;
            }

            // At this point we couldn't match any part of the symbol name in the AllKinds part of the list.
            // We now need to test the with the full name.
            Debug.Assert(symbolFullNameBuilder.Length > 0);
            Debug.Assert(symbolKindsToCheck.Count >= 1 && symbolKindsToCheck.Count <= 3);

            var symbolFullName = symbolFullNameBuilder.ToString();

            foreach (var kind in symbolKindsToCheck)
            {
                if (_wildcardNamesBySymbolKind.ContainsKey(kind) &&
                    _wildcardNamesBySymbolKind[kind].Any(x => symbolFullName.StartsWith(x, StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;

            bool MatchesSymbolPart(ISymbol symbol)
            {
                if (symbolFullNameBuilder.Length > 0)
                {
                    symbolFullNameBuilder.Insert(0, ".");
                }

                symbolFullNameBuilder.Insert(0, symbol.Name);

                return _wildcardNamesBySymbolKind.ContainsKey(AllKinds) &&
                    _wildcardNamesBySymbolKind[AllKinds].Any(x => symbol.Name.StartsWith(x, StringComparison.Ordinal));
            }
        }
    }
}