using System.Linq;
using EodtCore = EndOfDayTime.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EndOfDayTime.EntityFramework
{
    /// <summary>
    /// Extension methods that simplify configuration of <see cref="EodtCore.EndOfDayTime"/> 
    /// properties in EF Core models.
    /// </summary>
    public static class EndOfDayTimeModelBuilderExtensions
    {
        /// <summary>
        /// Configures this property to use <see cref="EndOfDayTimeValueConverter"/>,
        /// persisting the value as a smallint (minutes) in the database.
        /// </summary>
        public static PropertyBuilder<EodtCore.EndOfDayTime> HasEndOfDayTimeConverter(
            this PropertyBuilder<EodtCore.EndOfDayTime> builder)
        {
            return builder
                .HasConversion(new EndOfDayTimeValueConverter())
                .HasColumnType("smallint");
        }

        /// <summary>
        /// Automatically applies <see cref="EndOfDayTimeValueConverter"/> to all 
        /// <see cref="EodtCore.EndOfDayTime"/> properties in the model.
        /// Call this in OnModelCreating to avoid configuring each property individually.
        /// </summary>
        public static ModelBuilder ApplyEndOfDayTimeConverter(
            this ModelBuilder modelBuilder)
        {
            var converter = new EndOfDayTimeValueConverter();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(EodtCore.EndOfDayTime)))
                {
                    property.SetValueConverter(converter);
                }
            }
            return modelBuilder;
        }
    }
}