using Microsoft.AspNetCore.Mvc;

namespace PetsManagerMS.Controllers;

[ApiController]
[Route("")]
public class AppController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "PetsManagerMS works!",
            serverTime = DateTime.Now,
            globalTime = DateTime.UtcNow,
        });
    }
}