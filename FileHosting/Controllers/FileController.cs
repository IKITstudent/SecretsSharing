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

namespace FileHosting.Controllers
{
	public class FileController : Controller
	{
		private IWebHostEnvironment environment;
		private DBContext context;
		private FileActions fileActions;
		private string path;

		public FileController(DBContext dBContext, IWebHostEnvironment environment)
		{
			fileActions = new FileActions(dBContext);
			context = dBContext;
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
				return View(fileActions.GetCurrentFile());
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
			fileActions.SetCurrentFilePath(path);
			fileActions.DeleteFile(this.environment.WebRootPath);

			return RedirectToAction("Profile", "Account");
		}

		/// <summary>
		/// Adding file
		/// </summary>
		/// <param name="formFile"></param>
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
				fileActions.AddFile(formFile,HttpContext.User.Identity, environment.WebRootPath);
				////Get Current user
				//var user = HttpContext.User.Identity;

				////Path for creating file in folder
				//string FilePath = Path.Combine(environment.WebRootPath, "Files", user.Name);

				////Check for user directory
				//if (!Directory.Exists(FilePath))
				//{
				//	Directory.CreateDirectory(FilePath);
				//}

				////Create new file
				//Models.File file = new Models.File();

				////Filling fields
				//file.UserName = user.Name;
				//file.Author = context.Users.Where(u => u.UserName == user.Name).FirstOrDefault();
				//file.Title = formFile.Title;
				//file.TextData = formFile.TextData;

				//if (formFile.InputFile is not null)
				//	file.FileName = formFile.InputFile.FileName;
				//else
				//	file.FileName = null;

				//file.IsDelete = formFile.IsDeleted;
				//file.Views = 0;

				////Create a path by MD5
				//StringBuilder stringBuilder = new StringBuilder();
				//using (MD5 md5 = MD5.Create())
				//{
				//	byte[] hashValue = null;

				//	//Hash based at title of text ot file name if title is null
				//	if (formFile.InputFile is not null)
				//	{
				//		hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(formFile.InputFile.FileName));
				//	}
				//	else
				//	{
				//		hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(formFile.Title));
				//	}

				//	foreach (byte b in hashValue)
				//	{
				//		stringBuilder.Append($"{b:X2}");
				//	}
				//}

				//file.Path = stringBuilder.ToString();

				////Add user new file
				//context.Users.Where(u => u.UserName == user.Name).FirstOrDefault().Files.Add(file);

				//context.SaveChangesAsync();

				////Creating unploading file in folder
				//if (formFile.InputFile != null)
				//{
				//	using (var stream = new FileStream(Path.Combine(FilePath, file.FileName), FileMode.Create))
				//	{
				//		await formFile.InputFile.CopyToAsync(stream);
				//	}
				//}

				return RedirectToAction("Profile", "Account");
			}
		}
	}
}
