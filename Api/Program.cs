using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguration(builder.Configuration);
builder.Services.AddDbContexts(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddRateLimiting();
builder.Services.AddSwagger();
builder.Services.RegisterModules();

var app = builder.Build();

app.UseAuth();
app.UseRateLimting();
app.UseSwaggerIfDevelopment();
app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();

public partial class Program { }