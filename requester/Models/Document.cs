
namespace app
{   
    public class Documents{
        public Document[] documents;
    }

    public class Document
    {
        public string? id {get; set;}

        public string text {get; set;}

        public DocumentMetaData? metadata{get; set;}
    }

    public class DocumentMetaData
    {
        public string? source {get; set;}

        public string? source_id {get; set;}

        public string? url {get; set;}

        public string? created_at {get; set;}
        
        public string? author {get; set;}
    }

    public enum SourceType 
    {
        email = 0,
        file = 1,
        chat = 2,
    }
}