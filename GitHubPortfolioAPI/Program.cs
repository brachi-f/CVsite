using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);



// הוספת ה-GitHubService עם הזרקת התלויות שלו
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGitHubService, CachedGitHubService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHubSettings"));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
