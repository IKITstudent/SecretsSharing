using FileHosting.Data;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FileHosting.Controllers
{
    public class FileController : Controller
    {
        private DBContext _context;
        public FileController(DBContext dBContext)
        {
            _context = dBContext;
        }
        public IActionResult Info(string path)
        {
            var url = Request.GetDisplayUrl();
            Models.File model = _context.Files.FirstOrDefault(f => f.Path == path);
            model.Path = url;
            return View(model);
        }
    }
}
