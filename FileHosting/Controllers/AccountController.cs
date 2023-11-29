using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FileHosting.Models;
using System.Data;
using FileHosting.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;


namespace FileHosting.Controllers
{
    public class AccountController : Controller
    {
        private IWebHostEnvironment environment;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private FileActions _fileActions;
        private DBContext _context;

        IWebHostEnvironment _appEnvironment;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment appEnvironment, DBContext dBContext)
        {
            _context = dBContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _appEnvironment = appEnvironment;
            _fileActions = new FileActions(_context);
            _fileActions.CheckForViews(_appEnvironment.WebRootPath);
        }

        /// <summary>
        /// Profile page display
        /// </summary>
        /// <returns></returns>
        public IActionResult Profile()
        {
            //Get current user
            User CurrentUser = _context.Users.Where(u => u.UserName == HttpContext.User.Identity.Name).FirstOrDefault();

            //Get and send user files to view
            ViewBag.Files = _context.Files.Where(u => u.Author.Id == CurrentUser.Id).ToList();

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
                User user = new User { Email = model.Email, UserName = model.Email };

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
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

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
