using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChattApp.Data;
using MinimalChattApp.Model;

namespace MinimalChattApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public MessagesController(ChatDbContext context)
        {
            _context = context;
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessage()
        {
          if (_context.Message == null)
          {
              return NotFound();
          }
            return await _context.Message.ToListAsync();
        }

        // GET: api/Messages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
          if (_context.Message == null)
          {
              return NotFound();
          }
            var message = await _context.Message.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

        // PUT: api/Messages/5
        // To protect from o
        [HttpPut("{messageId}")]
        [Authorize] // Require authentication to access this endpoint
        public async Task<IActionResult> EditMessage(int messageId, MessageRequest messageRequest)
        {
            // Get the sender ID from the authenticated user making the request
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Check if the message exists and belongs to the authenticated user
            var message = await _context.Message.FindAsync(messageId);
            if (message == null)
            {
                return NotFound(new { error = "Message not found." });
            }

            if (message.SenderId != senderId)
            {
                return Unauthorized(new { error = "You can only edit your own messages." });
            }

            // Update the message content with the new content
            message.Content = messageRequest.Content;

            // Save the changes to the database
            _context.Message.Update(message);
            await _context.SaveChangesAsync();

            // Return a 200 OK response indicating the successful edit
            return Ok();
        }
    

    // POST: api/Messages
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
        [Authorize] // Require authentication to access this endpoint
        public async Task<IActionResult> SendMessage(MessageRequest messageRequest)
        {
            // Get the sender ID from the authenticated user making the request
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Check if the receiver user exists
            var receiver = await _context.User.FindAsync(messageRequest.ReceiverId);
            if (receiver == null)
            {
                return NotFound(new { error = "Receiver user not found." });
            }

            // Create a new Message entity
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = messageRequest.ReceiverId,
                Content = messageRequest.Content,
                Timestamp = DateTime.UtcNow
            };

            // Add the message to the database
            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            // Return the message details in the response body
            return Ok(new
            {
                messageId = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                timestamp = message.Timestamp
            });
        }
    

    // DELETE: api/Messages/5
    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            if (_context.Message == null)
            {
                return NotFound();
            }
            var message = await _context.Message.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Message.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(int id)
        {
            return (_context.Message?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
