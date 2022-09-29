
namespace API.Controllers;

//NG App is our API View 
public class FallbackController : Controller
{
  public ActionResult Index()
  {
    return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
    "wwwroot", "index.html"),
    "text/HTML"
    );
  }
}
