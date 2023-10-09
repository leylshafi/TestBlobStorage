namespace TestBlobStorage.Models
{
    public class File
    {
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
