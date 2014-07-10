using System.Collections.Generic;
using Knigoskop.Site.Models.Shared;
using Newtonsoft.Json;
using ProtoBuf;

namespace Knigoskop.Site.Models.Api
{
    [ProtoContract]
    public class CatalogueApiResponse
    {
        [ProtoMember(1)]
        [JsonProperty(PropertyName = "items")]
        public IList<SearchSuggestionApiModel> Items { get; set; }
    }
}