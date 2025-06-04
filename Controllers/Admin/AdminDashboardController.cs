using Microsoft.AspNetCore.Mvc;

namespace Endterm_IPT.Controllers.Admin
{
    public class AdminDashboardController : Controller
    {
        
        public IActionResult AdminDashboard()
        {
            return View();
        }
    }
}
