using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChattApp.Data;
using MinimalChattApp.Model;

namespace MinimalChattApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public LogsController(ChatDbContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetLog(DateTime? startTime = null, DateTime? endTime = null)
        {
            if (_context.Log == null)
            {
                return NotFound();
            }
            startTime ??= DateTime.Now.AddMinutes(-5);
            endTime ??= DateTime.Now;

            var logs = _context.Log
                .Where(u => DateTime.Parse(u.TimeStamp) >= startTime && DateTime.Parse(u.TimeStamp) <= endTime)
                .Select(u => new Log
                {
                    id = u.id,
                    IP = u.IP,
                    Name = u.Name,
                    Request = u.Request.Replace("\n", "").Replace("\"", "").Replace("\r", ""),
                    TimeStamp = u.TimeStamp
                })
                .ToList();

            if (logs.Count == 0)
            {
                return NotFound(new { message = "Logs not found" });
            }

            return Ok(new { Logs = logs });
            return await _context.Log.ToListAsync();
        }
     
    
        // GET: api/Logs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Log>> GetLog(int id)
        {
          if (_context.Log == null)
          {
              return NotFound();
          }
            var log = await _context.Log.FindAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return log;
        }

        // PUT: api/Logs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLog(int id, Log log)
        {
            if (id != log.id)
            {
                return BadRequest();
            }

            _context.Entry(log).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogExists(id))
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

        // POST: api/Logs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Log>> PostLog(Log log)
        {
          if (_context.Log == null)
          {
              return Problem("Entity set 'ChatDbContext.Log'  is null.");
          }
            _context.Log.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLog", new { id = log.id }, log);
        }

        // DELETE: api/Logs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            if (_context.Log == null)
            {
                return NotFound();
            }
            var log = await _context.Log.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            _context.Log.Remove(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogExists(int id)
        {
            return (_context.Log?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
