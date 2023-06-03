using FileHosting.Data;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FileHosting.Models;

namespace FileHosting.Controllers
{
    public class FileController : Controller
    {
        private Models.File _currentFile;
        private IWebHostEnvironment _environment;
        private DBContext _context;
        public FileController(DBContext dBContext, IWebHostEnvironment environment)
        {
            _context = dBContext;
            _environment = environment;

        }
        public IActionResult Info(string path)
        {
            var url = Request.GetDisplayUrl();
            _currentFile = _context.Files.FirstOrDefault(f => f.Path == path);
            _currentFile.Path = url;
            return View(_currentFile);
        }
        public FileResult DownloadFile(int id)
        {
            _currentFile = _context.Files.FirstOrDefault(f => f.Id == id);
            string path = Path.Combine(this._environment.WebRootPath, "Files", _currentFile.UserName, _currentFile.FileName);
            //byte[] bytes=System.IO.File.ReadAllBytes(path);
            return File(System.IO.File.OpenRead(path), "application/octet-stream", Path.GetFileName(path));
        }
    }
}
