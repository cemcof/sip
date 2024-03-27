using Microsoft.AspNetCore.Mvc;

namespace sip.Core;

public class MainController : Controller
{
    [Route("/")]
    public IActionResult Host()
    {
        return View("/Core/Host.cshtml");
    }
}