using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Create_Template.Controllers
{
    [Authorize(Roles ="Admin,Manager")]//Controller  içerisinde bulunan  tüm actionlara girişte login olup olmadığını kontrol eder.eğer sadece belirli actionlarda bu isteniyorsa   actipn bazlıda verilebilir.Rolse kavramı eklendiğinde rolü Uyan kişilerin yalnızca bu alana giriş yapabilmesini sağlar Sabit siring alabilirn sadece değişken kabul etmez

    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
