using AdoptionManagerMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models.Database;
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


builder.Services.AddCors(options =>
{
    options.AddPolicy("dev",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();

        });
});

builder.Services.AddScoped<AdopcionService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();