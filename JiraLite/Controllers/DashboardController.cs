using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Controllers;

public class DashboardController : Controller
{
    
    public IActionResult Index()
    {
        // Just return the view - authentication is handled by localStorage on frontend
        return View();
    }
}
