using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app.requester{
    public class Program{
        static async Task Main(string[] args){
            if(args.Count() == 0)
                throw new ArgumentException("Args contain no element");
            
            var requestUrl = "http://localhost:8000";
            const string UpsertFileAction = "/upsert-file";

            using(var client = new HttpClient()){
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("BEARER_TOKEN"));
                foreach(string arg in args){
                    var folderPath = arg;
                    Console.WriteLine($"---Current Folder: {folderPath}---");
                    string[] filePaths = Directory.GetFiles(folderPath);
                    foreach(var filePath in filePaths){
                        var fileName = Path.GetFileName(filePath);
                        Console.WriteLine($"---Current File: {fileName}---");
                        var content = new MultipartFormDataContent();
                        var stream = new FileStream(filePath, FileMode.Open);
                        content.Add(new StreamContent(stream), "file", fileName);
                        var res = await client.PostAsync(requestUrl + UpsertFileAction, content);
                        // res.EnsureSuccessStatusCode();
                        await PrettyPrintRes(res);
                    }
                }
            }
        }

        static async Task PrettyPrintRes(HttpResponseMessage res){
            var responseString = await res.Content.ReadAsStringAsync();
            var prettyResponseString = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseString), Formatting.Indented);
            Console.WriteLine(prettyResponseString);
        }
    }
}
