using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RobloxStudioModManager
{
    // Borrowed from Stack Overflow:
    // https://stackoverflow.com/questions/56933494/how-to-sort-properties-alphabetically-when-serializing-json-using-netwonsoft-lib

    internal class OrdinalSortJson : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var @base = base.CreateProperties(type, memberSerialization);

            var ordered = @base
                .OrderBy(p => p.PropertyName, StringComparer.Ordinal)
                .ToList();

            return ordered;
        }
    }
}
