using Create_Template.Entities;
using Create_Template.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;
using System.Security.Claims;

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
                string MD5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
                string hashedPassword = (model.Password + MD5Salt).MD5();
                User user= _databaseContext.Users.FirstOrDefault(x=>x.Username.ToLower()==model.UserName.ToLower()&&  x.Password == hashedPassword);
                if (user!=null)
                {
                    if (user.Locked)
                    {
                        ModelState.AddModelError(nameof(model.UserName), "Kullanıcı aktif değil sistem yöneticisine  başvurunuz");
                    }
                    else
                    {
                        #region CookieAut
                        //Cookie aut yaparken öncelikle cookie ye eklenecek verilere karar verilmeli
                        // bu verilerin her bir tanesine claim denir. Bu sebeple bir claim listesine ihtiyaç var
                        List<Claim> claims = new List<Claim>();
                        // Claim listesi oluşturulduktan sonra içerisine  atılacak claimlar bu şekilde eklenir.
                        claims.Add(new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()));
                        claims.Add(new Claim(ClaimTypes.Name,user.Name+user.Surname ?? string.Empty));
                        claims.Add(new Claim("Username",user.Username));
                        // ardından bir claim Identity classına ihtiyaç olmakta
                        ClaimsIdentity identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                        //Bunun ardından da bir claimPrincipal clasına ıhtuyac var
                        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                        // sonrasında  ise bir HttpContext  nesnesine ile sing in yapılamalı
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);
                        // Sonrasında  Program.cs  içerisinde  service builder ile  aut servisleri tanımlanmalı

                        #endregion
                        return RedirectToAction("Index", "Home");

                    }
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                }

                //  Login işlemler
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if(_databaseContext.Users.Any(x=>x.Username.ToLower()==model.UserName.ToLower())) 
                { 
                    ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı daha önce eklenmiş"); 
                }
                else
                {
                    string MD5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
                    string hashedPassword = (model.Password + MD5Salt).MD5();
                    

                    User user = new()
                    {
                        Username = model.UserName,
                        Password = hashedPassword
                        // Password Encirpt etmek için netcore.encryp  nugget paketi kullanılabilir.
                    };
                    _databaseContext.Users.Add(user);
                    int affectedRowCount = _databaseContext.SaveChanges();
                    if (affectedRowCount == 0)
                    {
                        ModelState.AddModelError("", "Kullanıcı Eklenemedi");
                    }
                    else if (affectedRowCount > 0)
                    {
                        return RedirectToAction(nameof(Login));
                    }

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
