    using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using FaizHesaplamaAPI.Data;
using FaizHesaplamaAPI.Services;
using FaizHesaplamaAPI.Converters;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);
// EPPlus Lisans ayarý (Community versiyonu için)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//AppDbContext'i tanýtma:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//FaizHesaplamaService'i tanýtma:
builder.Services.AddScoped<FaizHesaplamaService>();

// DynamicExcelProcessingService'i tanýtma:
builder.Services.AddScoped<DynamicExcelProcessor>();

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

// BAGLANTIYI SAGLAMA:
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
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
app.UseCors("AllowAngularDev");


app.MapControllers();

app.Run();
