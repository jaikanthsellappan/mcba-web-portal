using mcbaAdminAPI.Repositories;
using mcbaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mcbaAdminAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] //  Require JWT token
    public class PayeesController : ControllerBase
    {
        private readonly IPayeeRepository _repo;
        public PayeesController(IPayeeRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payee>>> GetAll() =>
            Ok(await _repo.GetAllAsync());

        [HttpGet("postcode/{postcode}")]
        public async Task<ActionResult<IEnumerable<Payee>>> GetByPostcode(string postcode) =>
            Ok(await _repo.GetByPostcodeAsync(postcode));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payee payee)
        {
            if (id != payee.PayeeID) return BadRequest();
            await _repo.UpdateAsync(payee);
            return NoContent();
        }
    }
}
