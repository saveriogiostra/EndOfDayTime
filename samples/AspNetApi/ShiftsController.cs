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
            // "HH:mm" strings bind to EndOfDayTime automatically; invalid values return 400.
            if (dto.End <= dto.Start)
                return BadRequest("End must be after start.");

            var shift = new WorkShift { Name = dto.Name, Start = dto.Start, End = dto.End };
            _db.Shifts.Add(shift);
            await _db.SaveChangesAsync();
            return Ok(shift);
        }
    }

    public class ShiftDto
    {
        public string Name  { get; set; } = string.Empty;
        public EodtCore.EndOfDayTime Start { get; set; }
        public EodtCore.EndOfDayTime End   { get; set; }
    }
}