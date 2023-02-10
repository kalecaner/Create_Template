using Create_Template.Entities;
using Create_Template.Models;
using Microsoft.AspNetCore.Mvc;

namespace Create_Template.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public AccountController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
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
                User user = new()
                {
                    Username = model.UserName,
                    Password = model.Password
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
