using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Profile;
using ICSharpCode.SharpZipLib.Zip;
using Knigoskop.Site.Code.Configuration;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;
using RestSharp.Extensions;
using System.Configuration;
using X.Text;

namespace Knigoskop.Site.Services
{
    public class ConvertService : IConvertService
    {
        private CalibreSettings _settings = (CalibreSettings)ConfigurationManager.GetSection("calibreConfiguration");

        private const BookFormatEnum DefaultBookFormat = BookFormatEnum.FB2;
        private const string ExternalAppName = "ebook-convert.exe";

        private string GetTempFileName(Guid identifier, BookFormatEnum outputFormat)
        {
            var fileName = string.Format("knigoskop_{0}.{1}", identifier, outputFormat.ToString().ToLower());
            return Path.Combine(Path.GetTempPath(), fileName);
        }

        private Guid ExtractBook(string fileName, byte[] sourceBook)
        {
            Guid id = Guid.NewGuid();
            using (var stream = new MemoryStream(sourceBook))
            {
                using (var zip = new ZipFile(stream))
                {
                    var entry = zip.GetEntry(fileName);
                    using (var data = zip.GetInputStream(entry))
                    {
                        using (var fileStream = File.Create(GetTempFileName(id, DefaultBookFormat)))
                        {
                            data.CopyTo(fileStream);
                        }
                    }
                }
            }
            return id;
        }

        private void Convert(Guid id, BookFormatEnum outputFormat)
        {
            var info = new ProcessStartInfo(string.Format("\"{0}\"", Path.Combine(_settings.Path, ExternalAppName)))
            {
                UseShellExecute = true,
                Arguments = string.Format("\"{0}\" \"{1}\"", GetTempFileName(id, DefaultBookFormat), GetTempFileName(id, outputFormat)),
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                CreateNoWindow = true,
            };
            var process = Process.Start(info);
            process.WaitForExit(1 * 60 * 1000);
        }

        public ConversionResultModel Compress(ConversionResultModel source)
        {

            var fileName = string.Format("{0}.zip", source.FullFileName);
            using (var zip = ZipFile.Create(fileName))
            {
                zip.BeginUpdate();
                zip.Add(source.FullFileName, source.DownloadFileName);
                zip.CommitUpdate();
            }
            if (File.Exists(source.FullFileName))
                File.Delete(source.FullFileName);
            source.FullFileName = fileName;
            source.DownloadFileName = string.Format("{0}.zip", source.DownloadFileName);
            return source;
        }

        public ConversionResultModel ConvertAndGetTargetFileName(BookSourceModel source, BookFormatEnum outputFormat,
            bool compress = true)
        {
            Guid id = ExtractBook(source.FileName, source.Body);
            if (outputFormat != DefaultBookFormat)
            {
                Convert(id, outputFormat);
                var sourceFile = GetTempFileName(id, DefaultBookFormat);
                if (File.Exists(sourceFile))
                    File.Delete(sourceFile);
            }

            var result = new ConversionResultModel
            {
                DownloadFileName = string.Format("{0}.{1}",
                    Transliterator.FromCyrillicToTransliteration(TextHelper.CleanCharacters(source.Name)),
                    outputFormat).ToLower(),
                FullFileName = GetTempFileName(id, outputFormat)
            };

            if (compress)
                result  = Compress(result);

            return result;
        }

    }
}