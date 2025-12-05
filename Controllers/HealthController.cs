using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;

namespace TeamCashCenter.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "healthy" });

        [HttpGet("ready")]
        public async Task<IActionResult> Ready([FromServices] CashCenterContext db)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync("SELECT 1");
                return Ok(new { ready = true });
            }
            catch
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
