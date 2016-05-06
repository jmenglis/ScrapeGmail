using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.CodeDom.Compiler;
using System.CodeDom;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Gmail.v1.Data;
using Newtonsoft.Json;

using ScrapeGmail;
using System;
using ExtensionMethods;

namespace ScrapeGmail.Controllers {
    public class AlchemyInfo {
        public int Mixed { get; set; }
        public float Score { get; set; }
        public string Type { get; set; }
    }
    public class SentItems {
        public string data { get; set; }
    }

    public class HomeController : Controller {
        string firstItem;
        public ActionResult Index() {
            return View();
        }
        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken) {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential != null) {
                var service = new GmailService(new BaseClientService.Initializer {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "ScrapeGmail"
                });
                List<Message> results = new List<Message>();
                UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
                request.LabelIds = "SENT";
                request.MaxResults = 1;
                ListMessagesResponse response = await request.ExecuteAsync();
                results.AddRange(response.Messages);
                request.PageToken = response.NextPageToken;
                firstItem = results[0].Id;

                //do {
                //    try {

                //    }
                //    catch (Exception e) {
                //        Debug.WriteLine("An error occurred: " + e.Message);

                //    }
                //} while (!String.IsNullOrEmpty(request.PageToken));
                //if (results != null && results.Count > 0) {
                //    firstItem = results[0].Id;
                //}
                Message myMessage = await service.Users.Messages.Get("me", firstItem).ExecuteAsync();
                MessagePart mailbody = myMessage.Payload;
                //Send Body of Gmail through to get the proper UTF8 Encoded Text
                string updateBody = GetMimeString(mailbody);

                //break up email string and remove any threads that are not part of the sent email.
                string resultingBody = "";
                string[] lines = Regex.Split(updateBody, "\n");
                foreach (string line in lines) {
                    if (!line.StartsWith(">")) {
                        if (!line.StartsWith("On") && !line.Contains("wrote:")) {
                            Regex trimmer = new Regex(@"\s+");
                            string newLine = trimmer.Replace(line, " ");
                            resultingBody += newLine;
                        }
                    }
                }
                string finalResult = resultingBody.Replace(System.Environment.NewLine, "");
                string testResult = finalResult.ToString();

                //trim down the weird spaces.
                ////Sending HTTP Request to Node.JS server.
                //WebRequest myRequest = WebRequest.Create("http://192.168.1.70:3000/");
                //myRequest.Method = "POST";
                //byte[] byteArray = Encoding.UTF8.GetBytes(resultingBody);
                //myRequest.ContentType = "application/x-www-form-urlencoded";
                //myRequest.ContentLength = byteArray.Length;
                //Stream dataStream = myRequest.GetRequestStream();
                //dataStream.Write(byteArray, 0, byteArray.Length);
                //dataStream.Close();
                //WebResponse myResponse = myRequest.GetResponse();
                //Debug.WriteLine(((HttpWebResponse)myResponse).StatusDescription);
                //dataStream = myResponse.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                //string responseFromServer = reader.ReadToEnd();
                //Debug.WriteLine(responseFromServer);
                //reader.Close();
                //dataStream.Close();
                //myResponse.Close();


                // HTTP Post Request to Node.JS Server
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://hrretentiontool.herokuapp.com:443/");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    //string json = "{ \"data\":" + "\"" + finalResult + "\"}";
                    //List<SentItems> mailItems = new List<SentItems> {
                    //    new SentItems {Data = finalResult }
                    //};
                    //string json = mailItems.ToJSON();
                    SentItems mailItems = new SentItems();
                    mailItems.data = finalResult;
                    string json = JsonConvert.SerializeObject(mailItems);
                    Debug.WriteLine(json);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    var jsonResult = streamReader.ReadToEnd();
                    Debug.WriteLine(jsonResult);
                    AlchemyInfo alchemy = JsonConvert.DeserializeObject<AlchemyInfo>(jsonResult);
                    ViewBag.Mixed = alchemy.Mixed;
                    ViewBag.Score = alchemy.Score;
                    ViewBag.Type = alchemy.Type;
                }
                return View();
            }
            else {
                return new RedirectResult(result.RedirectUri);
            }
        }
        //Code that decodes the Base64 Gmail Body to UTF8 Encoded Text
        public static string GetMimeString(MessagePart Parts) {
            string Body = "";
            if (Parts.Parts != null) {
                foreach (MessagePart part in Parts.Parts) {
                    Body = String.Format("{0}\n{1}", Body, GetMimeString(part));
                }
            }
            else if (Parts.Body.Data != null && Parts.Body.AttachmentId == null && Parts.MimeType == "text/plain") {
                String codedBody = Parts.Body.Data.Replace("-", "+");
                codedBody = codedBody.Replace("_", "/");
                byte[] data = Convert.FromBase64String(codedBody);
                Body = Encoding.UTF8.GetString(data);
            }
            return Body;
        }
        // Legacy Code that is not called in my file and can be removed in the future.
        private static string ToLiteral(string input) {
            using (var writer = new StringWriter()) {
                using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
        //Remove Hidden Control Characters in the text.
        public static string RemoveControlCharacters(string inString) {
            if (inString == null) return null;
            StringBuilder newString = new StringBuilder();
            char ch;
            for (int i = 0; i < inString.Length; i++) {
                ch = inString[i];
                if (!char.IsControl(ch)) {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }
    }
}