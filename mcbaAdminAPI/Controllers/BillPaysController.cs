using mcbaAdminAPI.Repositories;
using mcbaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mcbaAdminAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] //  Require JWT token
    public class BillPaysController : ControllerBase
    {
        private readonly IBillPayRepository _repo;
        public BillPaysController(IBillPayRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillPay>>> GetAll() =>
            Ok(await _repo.GetAllAsync());

        [HttpPut("block/{id}")]
        public async Task<IActionResult> Block(int id)
        {
            await _repo.BlockAsync(id);
            return NoContent();
        }

        [HttpPut("unblock/{id}")]
        public async Task<IActionResult> Unblock(int id)
        {
            await _repo.UnblockAsync(id);
            return NoContent();
        }
    }
}
