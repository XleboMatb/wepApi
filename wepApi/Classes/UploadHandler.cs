using wepApi.Controllers;
using wepApi.Models;

namespace wepApi.Classes
{
    public class UploadHandler
    {

        public string UploadData(IFormFile file)
        {
            Value uploadedFile = new Value();
            List<string> extensions = new List<string>() { ".csv"};
            string ext = Path.GetExtension(file.FileName);
            if (!extensions.Contains(ext))
            {
                return "Extension is not valid! you can only add .csv file!";
            }

            if (file.Length > 1024*1024)
            {
                return "Max size can only be 1mb";
            }


            return "File is fine";
        }

    }
}
