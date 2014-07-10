using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Knigoskop.Services.ZipFunctions
{
    public class UnzipFeatures
    {
        public static void UnzipFileToFolder(string zipFileName, string destinationFolder)
        {
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileName)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    string directoryName = destinationFolder;//Path.GetDirectoryName(theEntry.Name);
                    string fileName = directoryName + "//" + Path.GetFileName(theEntry.Name);

                    if (fileName != String.Empty && Path.GetExtension(fileName).ToLower().Equals(".inp"))
                    {
                        using (FileStream streamWriter = File.Create(fileName))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
