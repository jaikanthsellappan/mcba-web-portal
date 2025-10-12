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

        [HttpGet("{id}")]
        public async Task<ActionResult<Payee>> GetById(int id)
        {
        var payee = await _repo.GetByIdAsync(id);
        if (payee == null)
            return NotFound();  // returns 404 if invalid ID

        return Ok(payee);
        }

        [HttpPut("{id}")]
public async Task<IActionResult> Update(int id, Payee payee)
{
    Console.WriteLine($"[API] Received PUT for PayeeID={id}");

    if (id != payee.PayeeID)
    {
        Console.WriteLine("[API] Mismatched PayeeID!");
        return BadRequest("Mismatched Payee ID.");
    }

    try
    {
        await _repo.UpdateAsync(payee);
        Console.WriteLine("[API] Payee updated successfully.");
        return Ok(new { message = "Updated successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[API] Update Error: {ex.Message}");
        return StatusCode(500, new { error = ex.Message });
    }
}

    }
}
