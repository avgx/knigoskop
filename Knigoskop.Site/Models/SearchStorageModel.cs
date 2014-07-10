using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{
    public enum SearchStorageTypeEnum
    {
        FileSystem = 0,
        Azure = 1
    }

    public class SearchStorageModel
    {
        public string DirectoryName { get; set; }
        public SearchStorageTypeEnum StorageType { get; set; }
    }
}