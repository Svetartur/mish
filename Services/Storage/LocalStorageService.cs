namespace ASP_P42.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        private readonly String[] allowedExtensions = [".jpg", ".png", ".jpeg", ".webp"];
        private readonly String localFolder = "LocalStorage";

        public byte[] Load(string filename)
        {
            return File.ReadAllBytes(
                Path.Combine(localFolder, filename)
            );
        }

        public string Save(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(
                nameof(file), "Data not received");

            if (file.Length < 256) throw new ArgumentException("File too short");
            if (file.Length > 1e7) throw new ArgumentException("File too long");
            int dotPosition = file.FileName.LastIndexOf('.');
            if (dotPosition < 0) throw new ArgumentException("File must have extension");
            String ext = file.FileName[dotPosition..];
            if (!allowedExtensions.Contains(ext))
            {
                throw new ArgumentException("File type not allowed");
            }
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }
            String savedName = Guid.NewGuid() + ext;
            using FileStream stream = File.OpenWrite(
                Path.Combine(localFolder, savedName)
            );
            file.CopyTo(stream);
            return savedName;
        }
    }
}
