namespace wepApi.Models
{
    public class File
    {
        private int _fileID { get; set; }
        private string _fileName { get; set; }

        public int FileID { get { return _fileID; } set { _fileID = value; } }
        public string FileName { get { return _fileName; } set { _fileName = value; } }
    }
}
