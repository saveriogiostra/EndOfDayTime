using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Sample.AspNet.Models
{
    public class ShiftViewModel
    {
        public EodtCore.EndOfDayTime Start { get; set; }
        public EodtCore.EndOfDayTime End { get; set; }
        public string? Result { get; set; }
    }
}