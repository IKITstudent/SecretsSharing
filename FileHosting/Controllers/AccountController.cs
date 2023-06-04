using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FileHosting.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data;
using FileHosting.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;

namespace FileHosting.Controllers
{
    public class AccountController:Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private DBContext _context;
        
        IWebHostEnvironment _appEnvironment;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment appEnvironment, DBContext dBContext)
        {
            _context= dBContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _appEnvironment = appEnvironment;
            CheckFilesForDelete(_context);
		}

		/// <summary>
		/// This method deleting files if thiers view count >=1.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		private async Task<IActionResult> CheckFilesForDelete(DBContext dbContext)
        {
            //Getting files
            List<Models.File> files = dbContext.Files.ToList<Models.File>();

            //List pathes for deleting by pathes
            List<string> pathes = new List<string>();

            //Search files with views count>=1
            foreach (var file in files)
            {
                if (file.IsDelete && file.Views > 0)
                    pathes.Add(file.Path);
            }

            //Deleting files
            foreach (var path in pathes)
            {
				var RemFile = _context.Files.FirstOrDefault(f => f.Path == path);

				//Get file author
				User CurrentUser = _context.Users.FirstOrDefault(u => u.Email == RemFile.UserName);

				//Checking file for content in folder
				if (RemFile.FileName != null)
				{
					//Getting folder path to file
					string FilePath = Path.Combine(_appEnvironment.WebRootPath, "Files", CurrentUser.UserName, RemFile.FileName);

					//Deleting file from folder
					FileInfo fileInfo = new FileInfo(FilePath);
					fileInfo.Delete();
				}

				//Deleting file from databases
				CurrentUser.Files.Remove(RemFile);
				_context.Files.Remove(RemFile);
				_context.SaveChanges();

				
			}
			return RedirectToAction("Profile", "Account");
		}

        /// <summary>
        /// Profile page display
        /// </summary>
        /// <returns></returns>
        public IActionResult Profile()
        {
            //Get current user
            var user = HttpContext.User.Identity;
            User CurrentUser = _context.Users.Where(u => u.UserName == user.Name).FirstOrDefault();
            

            //Get user files
            List<Models.File> UserFiles = _context.Files.Where(u => u.Author.Id == CurrentUser.Id).ToList();

            //Send user files to view
            ViewBag.Files = UserFiles;

            return View();
        }

		/// <summary>
        /// Registration of new user
        /// </summary>
        /// <returns></returns>
		[HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            //Checking Registraton model for validation
            if (ModelState.IsValid)
            {
                //Create new user
                User user = new User { Email = model.Email, UserName = model.Email};
                
                //Add new user to database
                var result = await _userManager.CreateAsync(user, model.Password);

                //Checking adding result
                if (result.Succeeded)
                {
					//Setting cookies
					await _signInManager.SignInAsync(user, false);

                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
			//Checking Login model for validation
			if (ModelState.IsValid)
            {
                //Sign in user
                var result =await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

                //Cheking singn in result
                if (result.Succeeded)
                {
                    //Checking Url
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Profile", "Account");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        /// <summary>
        /// Log out user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Deleting athentification coockies
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
