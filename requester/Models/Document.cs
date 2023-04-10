
namespace app
{
    public class Document
    {
        public string? id {get; set;}

        public string text {get; set;}

        public DocumentMetaData? metaData {get; set;}
    }

    public class DocumentMetaData
    {
        public string? source {get; set;}

        public string? source_id {get; set;}

        public string? url {get; set;}

        public string? created_at {get; set;}
        
        public string? author {get; set;}
    }
}