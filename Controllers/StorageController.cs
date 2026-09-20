using ASP_P42.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Controllers
{
    public class StorageController(IStorageService storageService) : Controller
    {
        private readonly IStorageService _storageService = storageService;

        [HttpGet]
        public IActionResult Image(String id)
        {
            int dotPosition = id.LastIndexOf('.');
            String ext = "";
            if (dotPosition > 0)
            {
                ext = id[dotPosition..].ToLower();
            }
            String contentType = ext switch {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                ".tiff" or ".tif" => "image/tiff",
                ".avif" => "image/avif",
                _ => "application/octet-stream"
            };
            try
            {
                return File(_storageService.Load(id), contentType);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
