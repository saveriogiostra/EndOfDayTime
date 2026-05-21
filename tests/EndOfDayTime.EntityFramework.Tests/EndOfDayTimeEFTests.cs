using System;
using System.Linq;
using EndOfDayTime.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.EntityFramework.Tests
{
    // ── Test model ───────────────────────────────────────────────────────

    public class WorkShift
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EodtCore.EndOfDayTime Start { get; set; }
        public EodtCore.EndOfDayTime End { get; set; }
    }

    // ── DbContext using HasEndOfDayTimeConverter (per-property) ──────────

    public class ShiftDbContextManual : DbContext
    {
        public DbSet<WorkShift> Shifts { get; set; } = null!;

        public ShiftDbContextManual(DbContextOptions<ShiftDbContextManual> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkShift>(entity =>
            {
                entity.Property(s => s.Start).HasEndOfDayTimeConverter();
                entity.Property(s => s.End).HasEndOfDayTimeConverter();
            });
        }
    }

    // ── DbContext using ApplyEndOfDayTimeConverter (automatic) ───────────

    public class ShiftDbContextAuto : DbContext
    {
        public DbSet<WorkShift> Shifts { get; set; } = null!;

        public ShiftDbContextAuto(DbContextOptions<ShiftDbContextAuto> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyEndOfDayTimeConverter();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    public static class TestDbContextFactory
    {
        public static ShiftDbContextManual CreateManual()
        {
            var options = new DbContextOptionsBuilder<ShiftDbContextManual>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var ctx = new ShiftDbContextManual(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        public static ShiftDbContextAuto CreateAuto()
        {
            var options = new DbContextOptionsBuilder<ShiftDbContextAuto>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var ctx = new ShiftDbContextAuto(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }
    }

    // ── Tests ────────────────────────────────────────────────────────────

    public class EndOfDayTimeEFTests
    {
        // ── Manual converter (HasEndOfDayTimeConverter) ──────────────────

        [Fact]
        public void Manual_SaveAndReload_PreservesTime()
        {
            using var ctx = TestDbContextFactory.CreateManual();

            ctx.Shifts.Add(new WorkShift
            {
                Name = "Morning",
                Start = new EodtCore.EndOfDayTime(9, 0),
                End = new EodtCore.EndOfDayTime(17, 30)
            });
            ctx.SaveChanges();

            var loaded = ctx.Shifts.Single();
            Assert.Equal(new EodtCore.EndOfDayTime(9, 0), loaded.Start);
            Assert.Equal(new EodtCore.EndOfDayTime(17, 30), loaded.End);
        }

        [Fact]
        public void Manual_SaveAndReload_EndOfDay()
        {
            using var ctx = TestDbContextFactory.CreateManual();

            ctx.Shifts.Add(new WorkShift
            {
                Name = "Late",
                Start = new EodtCore.EndOfDayTime(8, 0),
                End = EodtCore.EndOfDayTime.EndOfDay
            });
            ctx.SaveChanges();

            var loaded = ctx.Shifts.Single();
            Assert.True(loaded.End.IsEndOfDay);
            Assert.Equal("24:00", loaded.End.ToString());
        }

        [Fact]
        public void Manual_SaveAndReload_Midnight()
        {
            using var ctx = TestDbContextFactory.CreateManual();

            ctx.Shifts.Add(new WorkShift
            {
                Name = "Night",
                Start = new EodtCore.EndOfDayTime(0, 0),
                End = new EodtCore.EndOfDayTime(8, 0)
            });
            ctx.SaveChanges();

            var loaded = ctx.Shifts.Single();
            Assert.Equal(new EodtCore.EndOfDayTime(0, 0), loaded.Start);
        }

        [Fact]
        public void Manual_MultipleRecords_AllPreserved()
        {
            using var ctx = TestDbContextFactory.CreateManual();

            ctx.Shifts.AddRange(
                new WorkShift { Name = "A", Start = new EodtCore.EndOfDayTime(9, 0),  End = new EodtCore.EndOfDayTime(17, 0) },
                new WorkShift { Name = "B", Start = new EodtCore.EndOfDayTime(17, 0), End = EodtCore.EndOfDayTime.EndOfDay },
                new WorkShift { Name = "C", Start = new EodtCore.EndOfDayTime(0, 0),  End = new EodtCore.EndOfDayTime(9, 0) }
            );
            ctx.SaveChanges();

            var shifts = ctx.Shifts.OrderBy(s => s.Name).ToList();
            Assert.Equal(3, shifts.Count);
            Assert.True(shifts[1].End.IsEndOfDay);
        }

        // ── Auto converter (ApplyEndOfDayTimeConverter) ──────────────────

        [Fact]
        public void Auto_SaveAndReload_PreservesTime()
        {
            using var ctx = TestDbContextFactory.CreateAuto();

            ctx.Shifts.Add(new WorkShift
            {
                Name = "Morning",
                Start = new EodtCore.EndOfDayTime(9, 0),
                End = new EodtCore.EndOfDayTime(17, 30)
            });
            ctx.SaveChanges();

            var loaded = ctx.Shifts.Single();
            Assert.Equal(new EodtCore.EndOfDayTime(9, 0), loaded.Start);
            Assert.Equal(new EodtCore.EndOfDayTime(17, 30), loaded.End);
        }

        [Fact]
        public void Auto_SaveAndReload_EndOfDay()
        {
            using var ctx = TestDbContextFactory.CreateAuto();

            ctx.Shifts.Add(new WorkShift
            {
                Name = "Late",
                Start = new EodtCore.EndOfDayTime(8, 0),
                End = EodtCore.EndOfDayTime.EndOfDay
            });
            ctx.SaveChanges();

            var loaded = ctx.Shifts.Single();
            Assert.True(loaded.End.IsEndOfDay);
        }

        // ── ValueConverter directly ──────────────────────────────────────

        [Fact]
        public void ValueConverter_ToProvider_ReturnsMinutes()
        {
            var converter = new EndOfDayTimeValueConverter();
            var toProvider = converter.ConvertToProviderExpression.Compile();
            Assert.Equal((short)570, toProvider(new EodtCore.EndOfDayTime(9, 30)));
            Assert.Equal((short)1440, toProvider(EodtCore.EndOfDayTime.EndOfDay));
            Assert.Equal((short)0, toProvider(new EodtCore.EndOfDayTime(0, 0)));
        }

        [Fact]
        public void ValueConverter_FromProvider_ReturnsCorrectTime()
        {
            var converter = new EndOfDayTimeValueConverter();
            var fromProvider = converter.ConvertFromProviderExpression.Compile();
            Assert.Equal(new EodtCore.EndOfDayTime(9, 30), fromProvider(570));
            Assert.Equal(EodtCore.EndOfDayTime.EndOfDay, fromProvider(1440));
            Assert.Equal(new EodtCore.EndOfDayTime(0, 0), fromProvider(0));
        }

        [Fact]
        public void ValueConverter_RoundTrip_EndOfDay()
        {
            var converter = new EndOfDayTimeValueConverter();
            var toProvider = converter.ConvertToProviderExpression.Compile();
            var fromProvider = converter.ConvertFromProviderExpression.Compile();
            var original = EodtCore.EndOfDayTime.EndOfDay;
            var roundTripped = fromProvider(toProvider(original));
            Assert.Equal(original, roundTripped);
        }
    }
}