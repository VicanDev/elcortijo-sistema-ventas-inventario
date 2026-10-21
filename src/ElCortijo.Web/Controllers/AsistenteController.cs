using Microsoft.AspNetCore.Mvc;

namespace ElCortijo.Web.Controllers;

public class AsistenteController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
