using azure_openai_quickstart_rate_limiting.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Items["RateLimitExceeded"] = true;
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        context.HttpContext.Response.Redirect("generate-bio?message=rate-limit-exceeded");
    };

    options.AddFixedWindowLimiter(policyName: "GenerateBioPolicy", configureOptions =>
    {
        configureOptions.PermitLimit = 1; // Limit to 1 request per hour
        configureOptions.Window = TimeSpan.FromHours(1);
        configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        configureOptions.QueueLimit = 0; // No queue limit
    });
});

builder.Services.AddSingleton<GenerativeAIService>(provider =>
{
	string keyFromEnvironment = "cc67ba39c6ad4938ac2b8c689dfc7977";
	Uri endpoint = new Uri("https://openai-dev-000.openai.azure.com/");
	return new GenerativeAIService(keyFromEnvironment, endpoint);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.Use(async (context, next) =>
{
	if (context.Response.StatusCode == 429)
	{
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsync("{\"error\": \"Too many requests. Please try again later.\"}");
	}
	else
	{
		await next();
	}
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
