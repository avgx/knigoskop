using System.Collections.Generic;
using Newtonsoft.Json;
using ProtoBuf;

namespace Knigoskop.Site.Models.Api
{
    [ProtoContract]
    public class SearchSuggestionsApiResponse
    {
        [ProtoMember(1)]
        [JsonProperty(PropertyName = "items")]
        public IList<SearchSuggestionApiModel> Items { get; set; }
    }
}