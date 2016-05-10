using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMarket.RespondentImporter.lib
{
    public class ImportFile
    {
        private string[] AllowedExtensions { get; set; }
        private string UploadDestination { get; set; }
        private string Filename { get; set; }

        public ImportFile(string WebRootPath)
        {
            AllowedExtensions = new string[] { ".csv" };
            UploadDestination = $"{WebRootPath}\\upload\\";

            if (!System.IO.Directory.Exists(UploadDestination))
            {
                System.IO.Directory.CreateDirectory(UploadDestination);
            }
        }

        public async Task<string> Save(IFormFile file) {
            if (file.ContentDisposition != null) {
                //parse uploaded file
                var parsedContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                Filename = parsedContentDisposition.FileName.Trim('"');
                string uploadPath = UploadDestination + Filename;

                //check extension
                bool extension = this.VerifyFileExtension(uploadPath);
                if (VerifyFileExtension(uploadPath))
                {
                    await file.SaveAsAsync(uploadPath);
                }
                else Filename = string.Empty;
            }

            return Filename;
        }

        internal IList<string> ReadColumnHeaders()
        {
            IList<string> ilColumns = new List<string>();
            using (var importStream = new FileStream(UploadDestination + Filename, FileMode.Open))
            {
                var importReader = new StreamReader(importStream);
                var firstLine = importReader.ReadLine();
                foreach (var column in firstLine.Split(';'))
                {
                    ilColumns.Add(column.Trim('"'));
                }
                importReader.Close();
                importStream.Close();
            }
            return ilColumns;
        }

        public StreamReader ReadFile(string FileName) {
            return new StreamReader(new FileStream(UploadDestination + FileName, FileMode.Open));
        }

        private bool VerifyFileExtension(string path)
        {
            return AllowedExtensions.Contains(System.IO.Path.GetExtension(path));
        }

    }
}
