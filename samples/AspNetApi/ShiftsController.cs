using EodtCore = EndOfDayTime.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndOfDayTime.Sample.AspNetApi
{
    [ApiController]
    [Route("[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly ShiftDbContext _db;

        public ShiftsController(ShiftDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Shifts.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShiftDto dto)
        {
            if (!EodtCore.EndOfDayTime.TryParse(dto.Start, out var start))
                return BadRequest("Invalid start time.");
            if (!EodtCore.EndOfDayTime.TryParse(dto.End, out var end))
                return BadRequest("Invalid end time.");
            if (end <= start)
                return BadRequest("End must be after start.");

            var shift = new WorkShift { Name = dto.Name, Start = start, End = end };
            _db.Shifts.Add(shift);
            await _db.SaveChangesAsync();
            return Ok(new { shift.Id, shift.Name, Start = shift.Start.ToString(), End = shift.End.ToString() });
        }
    }

    public class ShiftDto
    {
        public string Name  { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End   { get; set; } = string.Empty;
    }
}