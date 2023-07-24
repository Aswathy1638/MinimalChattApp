using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalChattApp.Model;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    // Your data store for logs (e.g., database, in-memory list, etc.)
    private static readonly List<Log> _logs = new List<Log>();

    [HttpGet]
    [Authorize]
    public IActionResult GetLogs(string startTime = null, string endTime = null)
    {
        // Check authorization token
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        DateTime? parsedStartTime = null;
        DateTime? parsedEndTime = null;

        // Parse startTime and endTime to DateTime
        if (!string.IsNullOrEmpty(startTime) && DateTime.TryParse(startTime, out var startDateTime))
        {
            parsedStartTime = startDateTime;
        }

        if (!string.IsNullOrEmpty(endTime) && DateTime.TryParse(endTime, out var endDateTime))
        {
            parsedEndTime = endDateTime;
        }

        // Filter logs based on parsedStartTime and parsedEndTime
        var filteredLogs = _logs
            .Where(log =>
                (!parsedStartTime.HasValue || log.TimeStamp >= parsedStartTime.Value) &&
                (!parsedEndTime.HasValue || log.TimeStamp <= parsedEndTime.Value))
            .ToList();

        if (filteredLogs.Count == 0)
        {
            return NotFound("No logs found.");
        }

        return Ok(filteredLogs);
    }
}
