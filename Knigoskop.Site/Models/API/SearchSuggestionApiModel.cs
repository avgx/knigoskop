using System.Collections.Generic;
using System.ComponentModel;
using Knigoskop.Site.Models.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;

namespace Knigoskop.Site.Models.Api
{
    [ProtoContract]
    public class SearchSuggestionApiModel
    {
        private string _description;

        [ProtoMember(1)]
        [JsonProperty(PropertyName = "value")]
        public string Id { get; set; }

        [ProtoMember(2), DefaultValue(ItemTypeEnum.Book)]
        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof (StringEnumConverter))]
        public ItemTypeEnum Type { get; set; }

        [ProtoMember(3)]
        [JsonProperty(PropertyName = "label")]
        public string Title { get; set; }

        [ProtoMember(4)]
        [JsonProperty(PropertyName = "rating")]
        public double Rating { get; set; }

        [ProtoMember(5)]
        [JsonProperty(PropertyName = "hasImage")]
        public bool HasImage { get; set; }

        [ProtoMember(6)]
        [JsonProperty(PropertyName = "desc")]
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                {
                    if ((Type == ItemTypeEnum.Book || Type == ItemTypeEnum.Serie) && Authors != null)
                        _description = string.Join(", ", Authors);
                    else if (Type == ItemTypeEnum.Author)
                    {
                        if (BornYear != null)
                        {
                            _description = BornYear.ToString();
                            if (DeathYear != null)
                                _description += " - " + DeathYear.ToString();
                        }
                    }
                }
                return _description;
            }
            set { _description = value; }
        }

        internal float Score { get; set; }
        internal IEnumerable<string> Authors { get; set; }
        internal int? BornYear { get; set; }
        internal int? DeathYear { get; set; }
    }
}