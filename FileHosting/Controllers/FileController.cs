using FileHosting.Data;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FileHosting.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Collections.Generic;

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
			CheckForViews(_context);
		}

		private void CheckForViews(DBContext dBContext)
		{
			List<Models.File> files = dBContext.Files.ToList<Models.File>();
			List<string> pathes = new List<string>();
            foreach (var file in files)
            {
                if(file.IsDelete && file.Views>0)
					pathes.Add(file.Path);
            }
			foreach (var path in pathes)
			{
				DeleteFile(path);
			}
		}
		/// <summary>
		/// Display information about file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
        public IActionResult Info(string path)
        {
			//CheckForViews(_context);

			var url = Request.GetDisplayUrl();

			//Getting file
			_currentFile = _context.Files.Where(f => f.Path == path).FirstOrDefault();

			if (_currentFile == null)
				return RedirectToAction("Error", "Home");

			_currentFile.Views++;
			_context.SaveChanges();
			//Full file url
			_currentFile.Path = url;

			


            return View(_currentFile);
        }

		/// <summary>
		/// Download file by file id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        public FileResult DownloadFile(int id)
        {
			//Getting file id from datatabase
            _currentFile = _context.Files.FirstOrDefault(f => f.Id == id);

			//File path 
            string path = Path.Combine(this._environment.WebRootPath, "Files", _currentFile.UserName, _currentFile.FileName);

            return File(System.IO.File.OpenRead(path), "application/octet-stream", Path.GetFileName(path));
        }

		/// <summary>
		/// Deleting file by his path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> DeleteFile(string path)
		{
			//Get file from database
			var RemFile = _context.Files.FirstOrDefault(f => f.Path == path);

			//Get file author
			User CurrentUser = _context.Users.FirstOrDefault(u => u.Email == RemFile.UserName);

			//Checking file for content in folder
			if (RemFile.FileName != null)
			{
				//Getting folder path to file
				string FilePath = Path.Combine(_environment.WebRootPath, "Files", CurrentUser.UserName, RemFile.FileName);

				//Deleting file from folder
				FileInfo fileInfo = new FileInfo(FilePath);
				fileInfo.Delete();
			}

			//Deleting file from databases
			CurrentUser.Files.Remove(RemFile);
			_context.Files.Remove(RemFile);
			_context.SaveChanges();

			return RedirectToAction("Profile", "Account");
		}
		
		/// <summary>
		/// Adding file
		/// </summary>
		/// <param name="formFile"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> AddFile(FileModel formFile)
		{
			//Checking for null fields
			if (formFile.Title == null && formFile.TextData == null && formFile.InputFile == null)
			{
				return RedirectToAction("Profile", "Account");
			}
			else
			{
				//Get Current user
				var user = HttpContext.User.Identity;

				//Path for creating file in folder
				string FilePath = Path.Combine(_environment.WebRootPath, "Files", user.Name);

				//Check for user directory
				if (!Directory.Exists(FilePath))
				{
					Directory.CreateDirectory(FilePath);
				}

				//Create new file
				Models.File file = new Models.File();

				//Filling fields
				file.UserName = user.Name;
				file.Author = _context.Users.Where(u => u.UserName == user.Name).FirstOrDefault();
				file.Title = formFile.Title;
				file.TextData = formFile.TextData;

				if (formFile.InputFile is not null)
					file.FileName = formFile.InputFile.FileName;
				else
					file.FileName = null;

				file.IsDelete = formFile.IsDeleted;
				file.Views = 0;

				//Create a path by MD5
				StringBuilder stringBuilder = new StringBuilder();
				using (MD5 md5 = MD5.Create())
				{
					byte[] hashValue = null;
					
					//Hash based at title of text ot file name if title is null
					if (formFile.InputFile is not null)
					{
						hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(formFile.InputFile.FileName));
					}
					else
					{
						hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(formFile.Title));
					}

					foreach (byte b in hashValue)
					{
						stringBuilder.Append($"{b:X2}");
					}
				}

				file.Path = stringBuilder.ToString();

				//Add user new file
				_context.Users.Where(u => u.UserName == user.Name).FirstOrDefault().Files.Add(file);

				_context.SaveChangesAsync();

				//Creating unploading file in folder
				if (formFile.InputFile != null)
				{
					using (var stream = new FileStream(Path.Combine(FilePath, file.FileName), FileMode.Create))
					{
						await formFile.InputFile.CopyToAsync(stream);
					}
				}

				return RedirectToAction("Profile", "Account");
			}
		}
	}
}
