using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app.requester{
    public class Program{

        const string Url = "http://localhost:3333";
        const string QueryAction = "/query";
        const string UpsertAction = "/upsert";
        const string UpsertFileAction = "/upsert-file";

        static async Task Main(string[] args){

            if(args.Count() <= 1)
                throw new ArgumentException("Please pass the action and file paths as the args.");
            
            string action = args[0];
            Func<HttpClient, string, string, Task<HttpResponseMessage>> requester;

            switch (action){
                case "upsert": case "Upsert":
                    requester = Upsert;
                    action = UpsertAction;
                    break;
                case "upsertfile": case "UpsertFile": case "Upsertfile":
                    requester = UpsertFile;
                    action = UpsertFileAction;
                    break;
                case "query": case "Query":
                    requester = Query;
                    action = QueryAction;
                    break;
                default:
                    throw new ArgumentException($"Illegal input for action: {action}");
            }

            var paths = args.Skip(1).ToArray();
            await SendRequestForAllFilePath(paths, action, requester);
        }

        /// Send requests for all files under the filePath, depth = 1
        static async Task SendRequestForAllFilePath(string[] paths, string action, Func<HttpClient, string, string, Task<HttpResponseMessage>> requester){
            using(var client = new HttpClient()){
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("BEARER_TOKEN"));
                foreach(string path in paths){
                    var folderPath = path;
                    Console.WriteLine($"---Current Folder: {folderPath}---");
                    string[] filePaths = Directory.GetFiles(folderPath);
                    foreach(var filePath in filePaths){
                        var fileName = Path.GetFileName(filePath);
                        Console.WriteLine($"---Current File: {fileName}---");
                        
                        var res = await requester(client, filePath, fileName);

                        res.EnsureSuccessStatusCode();
                        await PrettyPrintRes(res);
                    }
                }
            }
        }

        static async Task<HttpResponseMessage> Query(
            HttpClient client,
            string filePath, 
            string fileName
        )
        {

            throw new NotImplementedException();
        }

        static async Task<HttpResponseMessage> Upsert(
            HttpClient client,
            string filePath, 
            string fileName
        )
        {

            throw new NotImplementedException();
        }

        static async Task<HttpResponseMessage> UpsertFile(
            HttpClient client,
            string filePath, 
            string fileName
        )
        {
            var content = new MultipartFormDataContent();
            var stream = new FileStream(filePath, FileMode.Open);
            content.Add(new StreamContent(stream), "file", fileName);
            var res = await client.PostAsync(Url + UpsertFileAction, content);
            return res;
        }

        static async Task PrettyPrintRes(HttpResponseMessage res){
            var responseString = await res.Content.ReadAsStringAsync();
            var prettyResponseString = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseString), Formatting.Indented);
            Console.WriteLine(prettyResponseString);
        }
    }
}
