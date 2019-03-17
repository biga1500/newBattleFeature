using Google.Apis.Auth.OAuth2;
using Google.Apis.Vision.v1;

using Google.Apis.Util.Store;

using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Services;
using Google.Apis.Vision.v1.Data;
using Newtonsoft.Json;
using System.Drawing;
using Image = Google.Apis.Vision.v1.Data.Image;

namespace OCRAPITest.Google
{
    public class Annotate
    {
        public string ApplicationName { get { return "Ocr"; } }

        public string JsonResult { get; set; }
        public string TextResult { get; set; }
        public string Error { get; set; }

        private GoogleCredential CreateCredential()
        {
            // this is the place to enter your own google API key (= json file). The app crashes without valid key. 
            using (var stream = new FileStream(Application.StartupPath + "\\My First Project-9d7476571afe.json", FileMode.Open, FileAccess.Read))
            {
                string[] scopes = { VisionService.Scope.CloudPlatform };
                var credential = GoogleCredential.FromStream(stream);
                credential = credential.CreateScoped(scopes);
                return credential;
            }
        }


        private VisionService CreateService(GoogleCredential credential)
        {
            var service = new VisionService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
                GZipEnabled = true,
            });

            return service;
        }

        /// <summary>
        /// read image as byte and send to google api
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="language"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<string> GetText(System.Drawing.Bitmap imagem, string language, string type)
        {
            TextResult = JsonResult = "";
            var credential = CreateCredential();
            var service = CreateService(credential);
            service.HttpClient.Timeout = new TimeSpan(1, 1, 1);
            byte[] file = ImageToByte(imagem);
           

            BatchAnnotateImagesRequest batchRequest = new BatchAnnotateImagesRequest();
            batchRequest.Requests = new List<AnnotateImageRequest>();
            batchRequest.Requests.Add(new AnnotateImageRequest()
            {
                Features = new List<Feature>() { new Feature() { Type = type, MaxResults = 1 }, },
                ImageContext = new ImageContext() { LanguageHints = new List<string>() { language } },
                Image = new Image() { Content = Convert.ToBase64String(file) }
            });

            var annotate = service.Images.Annotate(batchRequest);
            BatchAnnotateImagesResponse batchAnnotateImagesResponse = annotate.Execute();
            if (batchAnnotateImagesResponse.Responses.Any())
            {
                AnnotateImageResponse annotateImageResponse = batchAnnotateImagesResponse.Responses[0];
                if (annotateImageResponse.Error != null)
                {
                    if (annotateImageResponse.Error.Message != null)
                        Error = annotateImageResponse.Error.Message;
                }
                else
                    TextResult = annotateImageResponse.TextAnnotations[0].Description.Replace("\n", ";");
                
            }
            return "";
        }
        public static byte[] ImageToByte(System.Drawing.Bitmap img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
