using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
