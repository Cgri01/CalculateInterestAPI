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

// DynamicExcelProcessingService'i tan�tma:
builder.Services.AddScoped<DynamicExcelProcessor>();

// EmailService tan�tma
builder.Services.AddScoped<IEmailService , EmailService>();

// VerificationService Tan�tma
//builder.Services.AddScoped<IVerificationService , VerificationService>(); AddScoped: Her API iste�ine yeni servis olu�turur , dogrulama koduna farkl� instance , kay�t ol'a farkl� instance olusturdugu i�in kod e�le�mesi yap�lam�yordu. Dictionary her seferinde s�f�rlan�yordu
builder.Services.AddSingleton<IVerificationService, VerificationService>(); //AddSingleton: T�m uygulama boyunca tek verificationService kullan�l�yor , Dict sabit kal�yor , kod g�nderilirken ve kay�t yap�l�rken ayn� dicte eri�iyor.


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
