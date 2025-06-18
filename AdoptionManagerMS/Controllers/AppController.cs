using Microsoft.AspNetCore.Mvc;

namespace AdoptionManagerMS.Controllers;

[ApiController]
[Route("")]
public class AppController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "AdoptionManagerMS works!",
            serverTime = DateTime.Now,
            globalTime = DateTime.UtcNow,
        });
    }
}