using FileHosting.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FileHosting.Models;


namespace FileHosting.Controllers
{
    public class FileController : Controller
    {
        private IWebHostEnvironment environment;
        private FileActions fileActions;
        private string path;

        public FileController(DBContext dBContext, IWebHostEnvironment environment)
        {
            fileActions = new FileActions(dBContext);
            this.environment = environment;
        }

        /// <summary>
        /// Display information about file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IActionResult Info(string path)
        {
            this.path = path;

            fileActions.SetCurrentFilePath(this.path);

            if (fileActions.IsFileExists())
            {
                return View(fileActions.FileInfo());
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Download file by file id
        /// </summary>
        /// <param name="path"></param>
        /// <returns>File</returns>
        public FileResult DownloadFile(string path)
        {
            fileActions.SetCurrentFilePath(path);

            //File path 
            var filepath = fileActions.LocalFilePath(this.environment.WebRootPath);

            return File(System.IO.File.OpenRead(filepath), "application/octet-stream", Path.GetFileName(filepath));
        }

        /// <summary>
        /// Deleting file by his path
        /// </summary>
        /// <param name="path"></param>
        public IActionResult DeleteFile(string path)
        {
            if (User.Identity.IsAuthenticated)
            {
                fileActions.SetCurrentFilePath(path);
                fileActions.DeleteFile(this.environment.WebRootPath);
                return RedirectToAction("Profile", "Account");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Adding file
        /// </summary>
        /// <param name="formFile"></param>
        [HttpPost]
        public IActionResult AddFile(FileModel formFile)
        {
            if (User.Identity.IsAuthenticated)
            {
                //Checking for null fields
                if (formFile.Title == null && formFile.TextData == null && formFile.InputFile == null)
                {
                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    fileActions.AddFile(formFile, HttpContext.User.Identity, environment.WebRootPath);

                    return RedirectToAction("Profile", "Account");
                }
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
