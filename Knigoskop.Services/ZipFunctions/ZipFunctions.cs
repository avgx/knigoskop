using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using ICSharpCode.SharpZipLib.Core;

namespace Knigoskop.Services.ZipFunctions
{
    public class FB2ZipMethods
    {
        public static void UnZipFB2File(string archiveFileName, string fb2FileId, string outputFolder)
        {
            using (ZipFile zf = new ZipFile(File.OpenRead(archiveFileName)))
            {
                foreach (ZipEntry theEntry in zf)
                {

                    string directoryName = outputFolder;//Path.GetDirectoryName(theEntry.Name);
                    string fileName = directoryName + "\\" + Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName.Length > 0 && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != String.Empty && Path.GetFileName(fileName).ToLower().Equals(fb2FileId))
                    {
                        byte[] buffer = new byte[4096];
                        Stream zipStream = zf.GetInputStream(theEntry);
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                        break;
                    }
                }
            }
        }

        public static void UnZipFB2Archive(string archiveFileName, string outputFolder, bool onlyFB2, bool showProgress)
        {
            string directoryName = outputFolder;
            // create directory
            if (directoryName.Length > 0)
            {
                Directory.CreateDirectory(directoryName);
            }
            ZipFile zf = new ZipFile(File.OpenRead(archiveFileName));
            try
            {
                foreach (ZipEntry zipEntry in zf)
                {
                    string fileName = directoryName + "\\" + Path.GetFileName(zipEntry.Name);
                    if (IsFileNameCorrect(fileName, onlyFB2))
                    {
                        if (showProgress)
                        {
                            Console.WriteLine(string.Format("Extract file: {0}", Path.GetFileName(fileName)));
                        }
                        byte[] data = new byte[4096];
                        Stream zipStream = zf.GetInputStream(zipEntry);
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, data);
                        }
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }
        }

        private static bool IsFileNameCorrect(string fileName, bool onlyFB2)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (onlyFB2 && Path.GetExtension(fileName).ToLower().Equals(".fb2"))
                {
                    return true;
                }
                else
                {
                    if (onlyFB2)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public static byte[] ZipFB2File(string fb2FileName)
        {
            using (FileStream fs = new FileStream(fb2FileName, FileMode.Open, FileAccess.Read))
            {
                MemoryStream outputMemStream = new MemoryStream();
                ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                ZipEntry newEntry = new ZipEntry(Path.GetFileName(fb2FileName));
                newEntry.DateTime = DateTime.Now;

                zipStream.PutNextEntry(newEntry);

                StreamUtils.Copy(fs, zipStream, new byte[4096]);
                zipStream.CloseEntry();

                zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
                zipStream.Close();

                outputMemStream.Position = 0;
                byte[] byteArrayOut = outputMemStream.ToArray();
                return byteArrayOut;
            }
        }
    }
}
