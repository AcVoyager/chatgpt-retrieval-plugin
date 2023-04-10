using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app{
    public class Program{

        const string Url = "http://localhost:3333";
        const string QueryAction = "/query";
        const string UpsertAction = "/upsert";
        const string UpsertFileAction = "/upsert-file";
        const string Author = "Azure China Frontdoor Team";

        static string BearerToken;

        static async Task Main(string[] args){

            if(args.Count() < 2)
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
                case "debug":
                    Debug(args[1]);
                    return;
                default:
                    throw new ArgumentException($"Illegal input for action: {action}");
            }

            BearerToken = Environment.GetEnvironmentVariable("BEARER_TOKEN");
            if(BearerToken == null){
                throw new NullReferenceException("Bearer token cannot be null");
            }

            var path = args[1];
            await SendRequestForAllFilePath(path, action, requester);
        }

        /// Send requests for all files under the filePath, depth = 1
        static async Task SendRequestForAllFilePath(string rootPath, string action, Func<HttpClient, string, string, Task<HttpResponseMessage>> requester){
            using(var client = new HttpClient()){
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                Console.WriteLine($"------Iterating all files under {rootPath}------");
                foreach (string filePath in Directory.EnumerateFiles(rootPath, "*.md", SearchOption.AllDirectories))
                {
                    Console.WriteLine($"Current file: {filePath}");
                    var fileName = Path.GetFileName(filePath);
                    var res = await requester(client, filePath, fileName);
                    try{
                        res.EnsureSuccessStatusCode();
                    }
                    catch(HttpRequestException e){
                        Console.WriteLine(e.StackTrace);
                        continue;
                    }
                    await PrettyPrintRes(res);
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
            string text = await File.ReadAllTextAsync(filePath);
            Document document = new Document{
                text = text,
                metaData = new DocumentMetaData{
                    url = filePath,
                    created_at = DateTime.Now.ToShortTimeString(),
                    author = Author
                }
            };

            Documents documents = new Documents{
                documents = new Document[]{
                    document
                }
            };

            HttpContent content  = new StringContent(JsonConvert.SerializeObject(documents), Encoding.UTF8, "application/json");
            var res = await client.PostAsync(Url + UpsertAction, content);

            return res;
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

        static void Debug(string filePath)
        {
            foreach (string path in Directory.EnumerateFiles(filePath, "*.md", SearchOption.AllDirectories))
            {
                Console.WriteLine(path);
            }
        }
    }
}
