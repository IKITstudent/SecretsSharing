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
        }
        
        //public IActionResult File(int id)
        //{

        //    Models.File file=_context.Files.FirstOrDefault(f=> f.Id == id);
        //    //return View(file);
        //    return RedirectToAction("Info", "File", id);
        //}
        public IActionResult Profile()
        {
            var user = HttpContext.User.Identity;
            User CurrentUser = _context.Users.Where(u => u.UserName == user.Name).FirstOrDefault();
            List<Models.File> UserFiles = _context.Files.Where(u => u.Author.Id == CurrentUser.Id).ToList();
            List<(string, string, string, string,bool)> outUserFiles = new List<(string, string, string, string,bool)>();
            
            foreach(var file in UserFiles)
            {
                outUserFiles.Add((file.Title, file.TextData, file.FileName, file.Path,file.IsDelete));
            }
            
            
            ViewBag.Files = outUserFiles;
            return View();
        }

        #region Auth
        [HttpPost]
        public async Task<IActionResult> AddFile(FileModel formFile)
        {

            var user = HttpContext.User.Identity;
            Models.File file=new Models.File();

            string path = Path.Combine(_appEnvironment.WebRootPath, "Files", user.Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            file.Title = formFile.Title;
            file.TextData = formFile.TextData;

            if (formFile.InputFile is not null)
                file.FileName = formFile.InputFile.FileName;
            else
                file.FileName = null;
            file.IsDelete = formFile.IsDeleted;

            StringBuilder stringBuilder = new StringBuilder();
            using(MD5  md5 =MD5.Create())
            {
                byte[] hashValue = null;
                if (formFile.InputFile is not null)
                { hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(formFile.InputFile.FileName)); }
                else
                { hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(formFile.Title)); }

                foreach (byte b in hashValue)
                {
                    stringBuilder.Append($"{b:X2}");
                }
            }
            file.Path = stringBuilder.ToString();    

            User CurrUser=_context.Users.Where(u=>u.UserName==user.Name).FirstOrDefault();
            CurrUser.Files.Add(file);
            _context.Files.Add(file);
            _context.SaveChangesAsync();

            if (formFile.InputFile != null)
            {
                using (var stream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                {
                    await formFile.InputFile.CopyToAsync(stream);
                }
            }

            
            return RedirectToAction("Profile", "Account");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email};
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // установка куки
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

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
