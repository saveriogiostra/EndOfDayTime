using System;
using System.Linq;
using System.Reflection;
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
        /// Configures this nullable property to use <see cref="EndOfDayTimeValueConverter"/>,
        /// persisting the value as a nullable smallint (minutes) in the database.
        /// </summary>
        public static PropertyBuilder<EodtCore.EndOfDayTime?> HasEndOfDayTimeConverter(
            this PropertyBuilder<EodtCore.EndOfDayTime?> builder)
        {
            return builder
                .HasConversion(new EndOfDayTimeValueConverter())
                .HasColumnType("smallint");
        }

        /// <summary>
        /// Applies <see cref="EndOfDayTimeValueConverter"/> to every
        /// <see cref="EodtCore.EndOfDayTime"/> property (nullable or not) in the model.
        /// Call this in ConfigureConventions — this is the recommended way to map
        /// all properties at once.
        /// </summary>
        public static ModelConfigurationBuilder UseEndOfDayTime(
            this ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<EodtCore.EndOfDayTime>()
                .HaveConversion<EndOfDayTimeValueConverter>();
            return configurationBuilder;
        }

        /// <summary>
        /// Automatically applies <see cref="EndOfDayTimeValueConverter"/> to all
        /// <see cref="EodtCore.EndOfDayTime"/> properties of the entity types already in the model.
        /// Call this at the end of OnModelCreating. Prefer <see cref="UseEndOfDayTime"/>
        /// in ConfigureConventions, which also covers entity types discovered later.
        /// </summary>
        public static ModelBuilder ApplyEndOfDayTimeConverter(
            this ModelBuilder modelBuilder)
        {
            var converter = new EndOfDayTimeValueConverter();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
            {
                // EF does not discover properties of unmapped types by convention,
                // so look at the CLR type rather than entityType.GetProperties().
                var clrProperties = entityType.ClrType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                    .Where(p => (Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)
                        == typeof(EodtCore.EndOfDayTime))
                    .Where(p => !entityType.IsIgnored(p.Name));

                foreach (var clrProperty in clrProperties)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(clrProperty.PropertyType, clrProperty.Name)
                        .HasConversion(converter);
                }
            }
            return modelBuilder;
        }
    }
}