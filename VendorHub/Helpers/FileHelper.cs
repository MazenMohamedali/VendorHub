namespace VendorHub.Helpers
{
    public class FileHelper
    {
        public static async Task SaveFileAsync(string folderPath, string fileName, IFormFile file)
        {

            string fullFilePath = System.IO.Path.Combine(folderPath, fileName);
            using (FileStream fileStream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }

        public static async Task DeleteFileAsync(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
