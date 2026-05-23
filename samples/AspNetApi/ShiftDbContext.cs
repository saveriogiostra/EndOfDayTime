using EodtCore = EndOfDayTime.Core;
using EndOfDayTime.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace EndOfDayTime.Sample.AspNetApi
{
    public class WorkShift
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EodtCore.EndOfDayTime Start { get; set; }
        public EodtCore.EndOfDayTime End { get; set; }
    }

    public class ShiftDbContext : DbContext
    {
        public ShiftDbContext(DbContextOptions<ShiftDbContext> options) : base(options) { }
        public DbSet<WorkShift> Shifts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyEndOfDayTimeConverter();
        }
    }
}