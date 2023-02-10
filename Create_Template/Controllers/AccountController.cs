using Create_Template.Entities;
using Create_Template.Models;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;

namespace Create_Template.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;
       private readonly IConfiguration _configuration; // appSettings içerisinden veri almak için kullanılır


        public AccountController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                //  Login işlemler
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string MD5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
                string hashedPassword=(model.Password+MD5Salt).MD5();

                User user = new()
                {
                    Username = model.UserName,
                    Password =hashedPassword
                    // Password Encirpt etmek için netcore.encryp  nugget paketi kullanılabilir.
                };
                _databaseContext.Users.Add(user);
                 int affectedRowCount= _databaseContext.SaveChanges();
                    if (affectedRowCount==0)
                {
                    ModelState.AddModelError("", "Kullanıcı Eklenemedi");
                }
                else if (affectedRowCount>0)
                {
                    return RedirectToAction(nameof(Login));
                }



            }
            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}
