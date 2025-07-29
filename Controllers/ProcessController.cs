using Microsoft.AspNetCore.Mvc;
using FaizHesaplamaAPI.Data;
using Microsoft.EntityFrameworkCore;
using FaizHesaplamaAPI.Models;
using System.Runtime;
using System.Data;

namespace FaizHesaplamaAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProcessController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ProcessController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //GET: api/process
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Process>>> GetAllProcesses()
        {
            var processes = await _context.Process.ToListAsync();
            return Ok(processes);
        }

        //GET PROCESS BY ID:
        [HttpGet("{id}")]
        public async Task<ActionResult<Process>> GetProcessById(int id)
        {
            var process = await _context.Process.FindAsync(id);
            if (process == null)
            {
                return NotFound();
            }
            return process;
        }

        //GET PROCESS BY DATE:
        //[HttpGet("GetProcessByDate")]
        //public async Task<ActionResult<Models.Process>> GetProcessByDate([FromQuery] string firstDate, [FromQuery] string lastDate)
        //{
        //    if (!DateOnly.TryParse(firstDate, out var firstDayParsed) || !DateOnly.TryParse(lastDate, out var lastDayParsed))
        //    {
        //        return BadRequest("Invalid date format , try 'dd.MM.yyyy' or 'yyyy-MM-dd' ");
        //    }

        //    var process = await _context.Process
        //        .FirstOrDefaultAsync(p => p.ilkTarih == firstDayParsed || p.sonTarih == lastDayParsed);

        //    if (process == null)
        //    {
        //        return NotFound(new { message = "Process not found for the given date" });
        //    }

        //    return Ok(process);

        //}


        //POST: api/process
        [HttpPost]
        public async Task<ActionResult<Process>> CreateProcess([FromBody] Process process)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Process.Add(process);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProcessById), new { id = process.id }, process);
        }


        //[HttpPost]
        //public async Task<IActionResult> CreateProcess([FromBody] Models.Process process)
        //{
        //    if (process == null)
        //    {
        //        return BadRequest("Process data is null");
        //    }
        //    _context.Process.Add(process);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetProcessByDate), new { ilkTarih = process.ilkTarih, sonTarih = process.sonTarih, faizOrani = process.faizOrani }, process);
        //}


        // Put
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProcess(int id, [FromBody] Process process)
        {
            if (id != process.id)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(process).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DBConcurrencyException)
            {
                if (!ProcessExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool ProcessExist(int id)
        {
            return _context.Process.Any(e => e.id == id);
        }

        //DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcess(int id)
        {
            var process = await _context.Process.FindAsync(id);
            if (process == null)
            {
                return NotFound();
            }

            _context.Process.Remove(process);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        //GET BY DATE:
        [HttpGet("search")]

        public async Task<ActionResult<IEnumerable<Process>>> SearchProcess([FromQuery] string startingDate , [FromQuery] string endingDate)
        {
            if (!DateOnly.TryParse(startingDate, out var startingDateParsed) || !DateOnly.TryParse(endingDate , out var endingDateParsed))
            {
                return BadRequest("Invalid date format");

            }

            var processes = await _context.Process
                .Where(p => p.ilkTarih >= startingDateParsed &&
                           (p.sonTarih <= endingDateParsed || p.sonTarih == null))
                .ToListAsync();

            return processes;


        }

    }

}
