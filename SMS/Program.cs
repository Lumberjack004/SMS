using Microsoft.EntityFrameworkCore;
using SMS;
using SMS.Components;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 添加Ant Design服务
builder.Services.AddAntDesign();

// 配置HTTPS
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7286; // 指定HTTPS端口
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
});

builder.Services.AddDbContextFactory<CimContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CimContext")));

builder.Services.AddQuickGridEntityFrameworkAdapter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<SMS.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
