﻿using Pdf.Droid;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;


namespace Pdf
{
    public class FileEndpoint
    {
        //TODO - Gerer fichiers de mm nom
        public async Task<string> UploadFile(FileData fileData1, FileData fileData2)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StreamContent(fileData1.GetStream()), "\"file\"", $"\"{fileData1.FilePath}\"");
            content.Add(new StreamContent(fileData1.GetStream()), "\"file\"", $"\"{fileData2.FilePath}\"");

            var httpClient = new HttpClient();

            var uploadServiceBaseAdress = "http://10.0.2.2:51549/api/Files/Upload";

            var httpResponseMessage = await httpClient.PostAsync(uploadServiceBaseAdress, content);

            string fileName = await httpResponseMessage.Content.ReadAsStringAsync();

            return fileName;
        }

        public async Task<Stream> GetFileConcated(string fileName)
        {
            HttpClient httpClient = new HttpClient();

            var uploadServiceBaseAdress = "http://10.0.2.2:51549/api/Files/Upload";

            using (HttpResponseMessage response = await httpClient.GetAsync(uploadServiceBaseAdress + fileName))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStreamAsync();
                    return result;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
    }
}

