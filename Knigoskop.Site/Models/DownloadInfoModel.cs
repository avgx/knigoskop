using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.DataModel;

namespace Knigoskop.Site.Models
{
    public class DownloadInfoModel
    {
        public Guid BookId { get; set; }
        public bool HasContent { get; set; }
        public UserDownloadAccessEnum UserAccess { get; set; }

        public bool HasUserDownload {
            get
            {
                return UserAccess != UserDownloadAccessEnum.Forbidden;
            }
        }
        public string PrimaryEmail { get; set; }
        public IEnumerable<DeviceModel> Devices { get; set; }
    }
}