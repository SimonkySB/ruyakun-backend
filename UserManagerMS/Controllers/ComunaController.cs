using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Database;

namespace UserManagerMS.Controllers;


[ApiController]
[Route("comunas")]
public class ComunaController(AppDbContext db) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var res = await db.Comuna.AsNoTracking().ToListAsync();
        return Ok(res);
    }
}