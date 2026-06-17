using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SUP_Project_s32557.Api.Auth;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "System Uznawania Przychodów", Version = "v1" });
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic auth. Test accounts: admin/admin123 or user/user123"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basic", document)] = []
    });
});

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
builder.Services.AddAuthorization();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddSingleton<ICurrencyService, StaticCurrencyService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex is BusinessException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = ex?.Message ?? "Unexpected error" });
    });
});

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
