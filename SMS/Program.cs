using Microsoft.EntityFrameworkCore;
using SMS;
using SMS.Components;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using SMS.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 添加HttpClient服务
builder.Services.AddHttpClient();

// 添加Web API控制器支持
builder.Services.AddControllers();

// 添加API资源管理器（用于开发环境的API文档）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加Ant Design服务
builder.Services.AddAntDesign();

// 添加CORS支持
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 配置JSON序列化选项
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

// 配置HTTPS 运行项目时应该注释掉
//builder.Services.AddHttpsRedirection(options =>
//{
    //options.HttpsPort = 7286; // 指定HTTPS端口
    //options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
//});

builder.Services.AddDbContextFactory<SMS.CimContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CimContext")));

builder.Services.AddQuickGridEntityFrameworkAdapter();

var app = builder.Build();

// 确保数据库是最新的
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SMS.CimContext>();
    context.Database.EnsureCreated(); // 这将创建数据库和所有表（如果它们不存在）
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // 在开发环境中启用Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 添加全局异常处理中间件
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// 启用CORS
app.UseCors("AllowAll");

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 映射API控制器路由
app.MapControllers();

app.Run();
