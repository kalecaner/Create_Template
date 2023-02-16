using Create_Template.Entities;
using Create_Template.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using NETCore.Encrypt.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;

namespace Create_Template.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration; // appSettings içerisinden veri almak için kullanılır
        public AccountController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }

        [AllowAnonymous] // başında authorize  attributu bulunan  bir controller altına  bu attributu yazdığında  authorize  kontrolünü o acion için devre edışı bırakır
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = HashPassword(model.Password);
                User user = _databaseContext.Users.FirstOrDefault(x => x.Username.ToLower() == model.UserName.ToLower() && x.Password == hashedPassword);
                if (user != null)
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
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        claims.Add(new Claim(ClaimTypes.Name, user.Name + user.Surname ?? string.Empty));
                        claims.Add(new Claim("Username", user.Username));
                        claims.Add(new Claim(ClaimTypes.Role, user.Role));// Kullanıcı rollerinin cookie atılması esnasında kullanılmakta
                        // ardından bir claim Identity classına ihtiyaç olmakta
                        ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        //Bunun ardından da bir claimPrincipal clasına ıhtuyac var
                        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                        // sonrasında  ise bir HttpContext  nesnesine ile sing in yapılamalı
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        // Sonrasında  Program.cs  içerisinde  service builder ile  aut servisleri tanımlanmalı
                        //Not: logot olmadan cookie verileri güncellenemez

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

        private string HashPassword(string Password)
        {
            string MD5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
            string hashedPassword = (Password + MD5Salt).MD5();
            return hashedPassword;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.UserName.ToLower()))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı daha önce eklenmiş");
                }
                else
                {
                    string hashedPassword = HashPassword(model.Password);


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
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Profile()
        {
            ProfileNameLoader();
            ProfileSurnameLoader();
            ProfileEmailLoader();
            return View();
        }
        public IActionResult ChangePassword([Required][MinLength(6)] string NewPassword, string OldPassword, string RePassword)
        { // model kullanmak bu noktada daha mantıklı
            if (ModelState.IsValid)
            {
                User user = GetUser();
                if (NewPassword == RePassword)
                {
                    string hashedoldPassword = HashPassword(OldPassword);
                    if (user.Password == hashedoldPassword)
                    {
                        string hashedNewPassword= HashPassword(NewPassword);
                        user.Password = hashedNewPassword;
                       int RowCount= _databaseContext.SaveChanges();
                        if (RowCount > 0)
                        {
                            ViewData["Result"] = "PasswordChanged";
                            return View("Profile");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Şifre Değişimi esnasında problem oluştu.Şifre Değiştirilemiyor");
                        }
                        return View("Profile");
                    }
                    else
                    {
                        ModelState.AddModelError(OldPassword, "Eski şifreniz doğru değil");
                        return View("Profile");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Girdiğiniz şifreler birbiri ile uyuşmuyor");
                    return View("Profile");
                } 
            }
            return View("Profile");
        }



        private User GetUser()
        {
            Guid UserId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));//Kullanıcının Idsi Cookie de NameIdentifireda vardı buradan alarak işlemelere devam edebiliriz.
            User user = _databaseContext.Users.FirstOrDefault(x => x.Id == UserId);
            return user;
        }

        [HttpPost]
        public IActionResult ProfileChangeName([Required][StringLength(50)] string? Name)// Süreçleri yönetirken bellir bir kısma kadar model kullanmadan da bu işler yapılabilir. örnreği buradaki name   parametresi  profile.cs içerisindeki  Username inputuna ait name  elementine verilen isimden gelmektedir. Model yoksa   elementlerin isimlerine bakılarak işaretleniler Required attributu  ise bu sürecin validasyon kontrolünün yapılmasnı sağlamakta
        {
            if (ModelState.IsValid)
            {

                User user = GetUser();
                user.Name = Name;
                _databaseContext.SaveChanges();
                return RedirectToAction(nameof(Profile));
            }
            ProfileNameLoader();
            return View("Profile");
        }
        [HttpPost]
        public IActionResult ProfileChangeSurName([Required][StringLength(50)] string? SurName)// Süreçleri yönetirken bellir bir kısma kadar model kullanmadan da bu işler yapılabilir. örnreği buradaki name   parametresi  profile.cs içerisindeki  Username inputuna ait name  elementine verilen isimden gelmektedir. Model yoksa   elementlerin isimlerine bakılarak işaretleniler Required attributu  ise bu sürecin validasyon kontrolünün yapılmasnı sağlamakta
        {
            if (ModelState.IsValid)
            {
                User user = GetUser();
                user.Surname = SurName;
                _databaseContext.SaveChanges();
                return RedirectToAction(nameof(Profile));
            }

            ProfileSurnameLoader();
            return View("Profile");
        }
        [HttpPost]
        public IActionResult ProfileChangeEmail([Required][StringLength(50)] string? Email)// Süreçleri yönetirken bellir bir kısma kadar model kullanmadan da bu işler yapılabilir. örnreği buradaki name   parametresi  profile.cs içerisindeki  Username inputuna ait name  elementine verilen isimden gelmektedir. Model yoksa   elementlerin isimlerine bakılarak işaretleniler Required attributu  ise bu sürecin validasyon kontrolünün yapılmasnı sağlamakta
        {
            if (ModelState.IsValid)
            {
                User user = GetUser();
                user.Email = Email;
                _databaseContext.SaveChanges();
                return RedirectToAction(nameof(Profile));
            }


            return View("Profile");
        }

        private void ProfileEmailLoader()
        {
            User user = GetUser();
            ViewData["Email"] = user.Email;
        }

        private void ProfileSurnameLoader()
        {
            User user = GetUser();
            ViewData["SurName"] = user.Surname;
        }
        private void ProfileNameLoader()
        {
            User user = GetUser();
            ViewData["Name"] = user.Name; //illa model kullanmaya gerek yok  kullanıcının adını alıp Viewdata  ilede  iletimini sağlayabilirsin 
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
