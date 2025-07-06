using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models.Database;
using PetsManagerMS.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new StringTrimmerJsonConverter());
    })
    .ConfigureApiBehaviorOptions(opt =>
    {
        IActionResult Data(ActionContext context)
        {
            ProblemDetailsFactory? problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            ValidationProblemDetails problemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState, 422);
            return new ObjectResult(problemDetails) { StatusCode = 422 };
        }

        opt.InvalidModelStateResponseFactory = Data;
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseOracle(builder.Configuration.GetConnectionString("db"));
});


builder.Services.AddAppCors();

builder.Services.AddScoped<IOrganizacionService, OrganizacionService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddSingleton<ICloudinary>(new Cloudinary(new Account(
    builder.Configuration["CloudinaryCloudName"],
    builder.Configuration["CloudinaryApiKey"],
    builder.Configuration["CloudinarySecretKey"]
    )));


builder.Services.AddSecurityExtensions();

var app = builder.Build();

 
app.UseCors("app");

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();