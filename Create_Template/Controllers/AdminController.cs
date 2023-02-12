using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Create_Template.Controllers
{
    [Authorize]//Controller  içerisinde bulunan  tüm actionlara girişte login olup olmadığını kontrol eder.eğer sadece belirli actionlarda bu isteniyorsa   actipn bazlıda verilebilir.
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
