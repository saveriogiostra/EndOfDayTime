using EodtCore = EndOfDayTime.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EndOfDayTime.EntityFramework
{
    /// <summary>
    /// EF Core ValueConverter that persists <see cref="EodtCore.EndOfDayTime"/> 
    /// as a short (minutes, 0–1440) in the database.
    /// </summary>
    public class EndOfDayTimeValueConverter : ValueConverter<EodtCore.EndOfDayTime, short>
    {
        /// <summary>Initialises a new instance of <see cref="EndOfDayTimeValueConverter"/>.</summary>
        public EndOfDayTimeValueConverter()
            : base(
                v => (short)v.TotalMinutes,
                v => EodtCore.EndOfDayTime.FromMinutes(v))
        {
        }
    }
}