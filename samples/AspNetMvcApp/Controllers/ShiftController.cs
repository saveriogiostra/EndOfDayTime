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
            if (model.End <= model.Start)
            {
                ModelState.AddModelError("End", "End time must be after start time.");
                return View(model);
            }

            var duration = model.End - model.Start;
            model.Result = $"Shift saved! Start: {model.Start} End: {model.End} Duration: {(int)duration.TotalHours}h {duration.Minutes:D2}m";
            return View(model);
        }
    }
}