using azure_openai_quickstart_rate_limiting.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure rate limiting
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

// Fetch sensitive values from environment variables
builder.Services.AddSingleton<GenerativeAIService>(provider =>
{
	string keyFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? throw new Exception("Missing environment variable: AZURE_OPENAI_KEY");
	string endpointFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new Exception("Missing environment variable: AZURE_OPENAI_ENDPOINT");
	Uri endpoint = new Uri(endpointFromEnvironment);
	return new GenerativeAIService(keyFromEnvironment, endpoint);
});

// Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware for handling 429 Too Many Requests errors
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

// Configure the HTTP request pipeline
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