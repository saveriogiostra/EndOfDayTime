using EodtCore = EndOfDayTime.Core;
using EndOfDayTime.EntityFramework;
using EndOfDayTime.Sample.AspNetApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShiftDbContext>(opt =>
    opt.UseInMemoryDatabase("shifts"));

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();