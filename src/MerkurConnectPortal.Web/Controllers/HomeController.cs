using Microsoft.AspNetCore.Mvc;

namespace MerkurConnectPortal.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Error()
    {
        return View();
    }

    public IActionResult NotFound()
    {
        return View();
    }
}
