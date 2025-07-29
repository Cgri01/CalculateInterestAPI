using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using FaizHesaplamaAPI.Data;
using FaizHesaplamaAPI.Services;
using FaizHesaplamaAPI.Converters;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);
// EPPlus Lisans ayar� (Community versiyonu i�in)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//AppDbContext'i tan�tma:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//FaizHesaplamaService'i tan�tma:
builder.Services.AddScoped<FaizHesaplamaService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DATEONLYJSON CONVERTER EKLEME
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
