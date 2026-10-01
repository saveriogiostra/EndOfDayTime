using EndOfDayTime.Sample.AspNet.Models;
using Microsoft.AspNetCore.Mvc;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Sample.AspNet.Controllers
{
    public class ShiftController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ShiftViewModel());
        }

        [HttpPost]
        public IActionResult Index(ShiftViewModel model)
        {
            if (model.Start is null)
                ModelState.AddModelError("Start", "Start time is required.");
            if (model.End is null)
                ModelState.AddModelError("End", "End time is required.");
            if (model.Start is not { } start || model.End is not { } end)
                return View(model);

            if (end <= start)
            {
                ModelState.AddModelError("End", "End time must be after start time.");
                return View(model);
            }

            var duration = end - start;
            model.Result = $"Shift saved! Start: {start} End: {end} Duration: {(int)duration.TotalHours}h {duration.Minutes:D2}m";
            return View(model);
        }
    }
}