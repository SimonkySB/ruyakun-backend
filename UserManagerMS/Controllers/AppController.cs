using Microsoft.AspNetCore.Mvc;

namespace UserManagerMS.Controllers;


[ApiController]
[Route("")]
public class AppController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "UserManagerMS works!",
            serverTime = DateTime.Now,
            globalTime = DateTime.UtcNow,
        });
    }
}