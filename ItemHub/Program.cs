using ItemHub.Data;
using ItemHub.HealthChecks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ItemHub.Interfaces;
using ItemHub.Repository;
using ItemHub.Services;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Nest;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// --- Аутентификация и Авторизация ---
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.Cookie.Name = "authCookie";
                });

// --- База данных (Postgres) ---
builder.Services.AddDbContext<DataBaseContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Redis ---
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
    var configurationOptions = ConfigurationOptions.Parse(redisConfiguration!, true);
    // Настраиваем параметры подключения
    configurationOptions.ConnectTimeout = 1000; // 1 секунда
    configurationOptions.ConnectRetry = 1; // Без повторных попыток подключения
    configurationOptions.AbortOnConnectFail = true; // Немедленный отказ при недоступности
    configurationOptions.AsyncTimeout = 1000; // 1 секунда для асинхронных операций
    configurationOptions.SyncTimeout = 1000;
    options.ConfigurationOptions = configurationOptions;
});

// --- Elasticsearch ---
var elasticSection = builder.Configuration.GetSection("ElasticSettings");
var elasticUri = elasticSection.GetValue<string>("Uri");
var elasticUser = elasticSection.GetValue<string>("Username");
var elasticPass = elasticSection.GetValue<string>("Password");
var defaultIndex = elasticSection.GetValue<string>("DefaultIndex");

var settings = new ConnectionSettings(new Uri(elasticUri!))
    .BasicAuthentication(elasticUser, elasticPass) // Включаем Basic Auth при необходимости
    .DefaultIndex(defaultIndex); // Задаём индекс по умолчанию
// Если хотите отключить проверку сертификата (небезопасно! Только для девелопмента)
// settings.ServerCertificateValidationCallback((o, certificate, chain, errors) => true);

var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);


// --- DI ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICacheRepository, RedisCacheRepository>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IPageManagerService, PageManagerService>();
builder.Services.AddScoped<IItemSearchService, ItemSearchService>();
builder.Services.AddScoped<IMyCookieManager, CookieManager>(); 
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IItemApiService, ItemApiService>();
builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<IUserContext, UserContext>(); 
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IItemService, ItemService>(); 
builder.Services.AddSingleton<ElasticHealthCheck>();
builder.Services.AddSingleton<RedisHealthCheck>();
builder.Services.AddScoped<ElasticSearch>();

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