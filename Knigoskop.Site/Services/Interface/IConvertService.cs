using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Services.Interface
{
    public interface IConvertService
    {
       //string CompressAndGetTargetFileName(string sourceFileName, string entryName);
       //string ConvertAndGetTargetFileName(string fileName, byte[] sourceBook, BookFormatEnum outputFormat);

        ConversionResultModel ConvertAndGetTargetFileName(BookSourceModel source, BookFormatEnum outputFormat, bool compress = true);
    }
}
