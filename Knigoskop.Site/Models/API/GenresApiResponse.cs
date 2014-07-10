using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.Site.Models.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;

namespace Knigoskop.Site.Models.Api
{
    [ProtoContract]
    public class GenreApiModel
    {
        [ProtoMember(1)]
        [JsonProperty(PropertyName = "id")]
        public string GenreId { get; set; }
        [ProtoMember(2)]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [ProtoMember(3)]
        [JsonProperty(PropertyName = "level")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GenreLevelEnum Level { get; set; }
    }

    [ProtoContract]
    public class GenresApiResponse
    {
        [ProtoMember(1)]
        [JsonProperty(PropertyName = "items")]
        public IList<GenreApiModel> Items { get; set; }
    }
}