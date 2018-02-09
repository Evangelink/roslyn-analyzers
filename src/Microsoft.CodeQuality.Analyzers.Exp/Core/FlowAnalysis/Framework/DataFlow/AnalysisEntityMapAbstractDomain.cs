﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// An abstract domain implementation for analyses that store dictionary typed data.
    /// </summary>
    internal class AnalysisEntityMapAbstractDomain<TValue> : MapAbstractDomain<AnalysisEntity, TValue>
    {
        public AnalysisEntityMapAbstractDomain(AbstractValueDomain<TValue> valueDomain)
            : base(valueDomain)
        {
        }

        protected override IDictionary<AnalysisEntity, TValue> MergeCore(IDictionary<AnalysisEntity, TValue> map1, IDictionary<AnalysisEntity, TValue> map2)
        {
            Debug.Assert(map1 != null);
            Debug.Assert(map2 != null);

            var resultMap = new Dictionary<AnalysisEntity, TValue>();
            foreach (var entry1 in map1)
            {
                AnalysisEntity key1 = entry1.Key;
                TValue value1 = entry1.Value;
                var equivalentKeys2 = map2.Keys.Where(key => key.EqualsIgnoringInstanceLocation(key1));
                if (!equivalentKeys2.Any())
                {
                    resultMap.Add(key1, ValueDomain.UnknownOrMayBeValue);
                    continue;
                }

                foreach (AnalysisEntity key2 in equivalentKeys2)
                {
                    TValue value2 = map2[key2];
                    TValue mergedValue = ValueDomain.Merge(value1, value2);
                    if (key1.InstanceLocation.Equals(key2.InstanceLocation))
                    {
                        resultMap[key1] = mergedValue;
                    }
                    else
                    {
                        AnalysisEntity mergedKey = key1.WithMergedInstanceLocation(key2);
                        if (resultMap.TryGetValue(mergedKey, out var existingValue))
                        {
                            mergedValue = ValueDomain.Merge(mergedValue, existingValue);
                        }

                        resultMap[mergedKey] = mergedValue;
                    }
                }
            }

            foreach (var key2 in map2.Keys)
            {
                if (!resultMap.ContainsKey(key2))
                {
                    resultMap.Add(key2, ValueDomain.UnknownOrMayBeValue);
                }
            }

            return resultMap;
        }
    }
}