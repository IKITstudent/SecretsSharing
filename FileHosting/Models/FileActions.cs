using FileHosting.Data;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;


namespace FileHosting.Models
{
	public class FileActions
	{
		private DBContext context;
		private string path;
		private Models.File currentFile;

		public FileActions(DBContext context)
		{
			this.context = context;
			CheckForViewsAsync();
		}

		/// <summary>
		/// Check files wich marked IsDelete for views and start method delete for finded files.
		/// </summary>
		private void CheckForViewsAsync()
		{
			List<Models.File> files = context.Files.ToList<Models.File>();

			foreach (var file in files)
			{
				if (file.IsDelete && file.Views > 0)
				{
					DeleteFile(file.Path);
				}
			}
		}

		/// <summary>
		/// Increase count of views.
		/// </summary>
		private void FileViewed()
		{
			currentFile.Views++;
			context.SaveChanges();
		}

		/// <summary>
		/// Search choosen file.
		/// </summary>
		private void FindFile()
		{
			currentFile = context.Files.Where(f => f.Path == path).FirstOrDefault();
		}

		/// <summary>
		/// Check file at disk.
		/// </summary>
		/// <param name="filePath">Path to file.</param>
		/// <param name="fileModel">File model.</param>
		/// <returns>
		/// Bool.
		/// </returns>
		private bool IsFileExistOnDisk(string filePath, FileModel fileModel)
		{
			if (fileModel.InputFile is not null)
			{
				var files = Directory.GetFiles(filePath, fileModel.InputFile.FileName);

				if (files.Length > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Encode file path.
		/// </summary>
		/// <param name="fileModel">File model.</param>
		/// <returns>String path.</returns>
		private string PathEncoding(FileModel fileModel)
		{
			//Create a path by MD5
			StringBuilder stringBuilder = new StringBuilder();

			using (MD5 md5 = MD5.Create())
			{
				byte[] hashValue = null;

				//Hash based at title of text ot file name if title is null
				if (fileModel.InputFile is not null)
				{
					hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(fileModel.InputFile.FileName + DateTime.Now.ToString()));
				}
				else
				{
					hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(fileModel.Title + DateTime.Now.ToString()));
				}

				foreach (byte b in hashValue)
				{
					stringBuilder.Append($"{b:X2}");
				}
			}

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Upload file from server to disk/
		/// </summary>
		/// <param name="filePath">Path of file.</param>
		/// <param name="fileModel">File model.</param>
		private async void UploadToDiskToAsync(string filePath, FileModel fileModel)
		{
			var path = Path.Combine(filePath, fileModel.InputFile.FileName);
			using (var stream = new FileStream(path, FileMode.Create))
			{
				var f = fileModel.InputFile;
				await fileModel.InputFile.CopyToAsync(stream);
			}
		}

		/// <summary>
		/// Search needed file.
		/// </summary>
		/// <returns>
		/// Return file.
		/// </returns>
		private File GetCurrentFile()
		{
			return currentFile;
		}

		/// <summary>
		/// Check file at server.
		/// </summary>
		/// <returns></returns>
		public bool IsFileExists()
		{
			FindFile();

			if (currentFile == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Search current file path.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public void SetCurrentFilePath(string filePath)
		{
			path = filePath;
		}


		/// <summary>
		/// Return current file. Increase view count.
		/// </summary>
		/// <returns></returns>
		public File FileInfo()
		{
			if (IsFileExists())
			{
				FileViewed();
			}

			return GetCurrentFile();
		}

		/// <param name="WebRootPath"></param>
		/// <returns>
		/// Return file path on disk.
		/// </returns>
		public string LocalFilePath(string WebRootPath)
		{
			currentFile = GetCurrentFile();
			return Path.Combine(WebRootPath, "Files", currentFile.UserName, currentFile.FileName);
		}

		/// <summary>
		/// Delete file.
		/// </summary>
		/// <param name="WebRootPath"></param>
		public void DeleteFile(string WebRootPath)
		{
			var file = GetCurrentFile();

			User CurrentUser = context.Users.FirstOrDefault(u => u.Email == file.UserName);

			//Delete from disk
			if (file.FileName != null)
			{
				//Getting folder path to file
				string filePath = Path.Combine(WebRootPath, "Files", CurrentUser.UserName, file.FileName);

				//Deleting file from folder
				FileInfo fileInfo = new FileInfo(filePath);
				fileInfo.Delete();
			}

			//Delete from database
			CurrentUser.Files.Remove(file);
			context.Files.Remove(file);
			context.SaveChanges();
		}

		/// <summary>
		/// Uploading file.
		/// </summary>
		/// <param name="fileModel"></param>
		/// <param name="identityUser"></param>
		/// <param name="WebRootPath"></param>
		public void AddFile(FileModel fileModel, IIdentity identityUser, string WebRootPath)
		{
			//Path for creating file in folder
			string filePath = Path.Combine(WebRootPath, "Files", identityUser.Name);

			if (!IsFileExistOnDisk(filePath, fileModel))
			{
				//Check for user directory
				if (!Directory.Exists(filePath))
				{
					Directory.CreateDirectory(filePath);
				}

				File file = new File();

				//Filling fields
				file.UserName = identityUser.Name;
				file.Author = context.Users.Where(u => u.UserName == identityUser.Name).FirstOrDefault();
				file.Title = fileModel.Title;
				file.TextData = fileModel.TextData;

				if (fileModel.InputFile is not null)
				{
					file.FileName = fileModel.InputFile.FileName;
				}
				else
				{
					file.FileName = null;
				}

				file.IsDelete = fileModel.IsDeleted;
				file.Views = 0;
				file.Path = PathEncoding(fileModel);

				//Add user new file
				context.Users.Where(u => u.UserName == identityUser.Name).FirstOrDefault().Files.Add(file);

				context.SaveChanges();

				//Creating uploading file in folder
				if (fileModel.InputFile != null)
				{
					UploadToDiskToAsync(filePath, fileModel);
				}

			}
		}
	}
}