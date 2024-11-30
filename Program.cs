using ItemHub.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ItemHub.Interfaces;
using ItemHub.Repository;
using ItemHub.Services;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.Cookie.Name = "authCookie";
                });

builder.Services.AddDbContext<DataBaseContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddStackExchangeRedisCache(options => 
    options.Configuration = builder.Configuration.GetConnectionString("Redis"));

// var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
//     .DefaultIndex("items"); // Задайте имя индекса
//
// var client = new ElasticClient(settings);
//
// builder.Services.AddSingleton<IElasticClient>(client);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICacheRepository, RedisCacheRepository>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IPageManagerService, PageManagerService>();
builder.Services.AddScoped<IMyCookieManager, CookieManager>(); 
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IItemApiService, ItemApiService>();
builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<IUserContext, UserContext>(); 
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IItemService, ItemService>(); 

// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}
else
{
    app.UseExceptionHandler(applicationBuilder => 
        {
            applicationBuilder.Run(async(context) => 
            {
                //пытаемся получить информацию об исключении
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature != null)
                    await context.Response.WriteAsync($"Error: {exceptionHandlerPathFeature.Error.Message}");
                else
                    await context.Response.WriteAsync("Unknown Error");
            });
        });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

WebRootPath.Path = app.Environment.WebRootPath;

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();