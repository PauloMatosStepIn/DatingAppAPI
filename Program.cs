
var builder = WebApplication.CreateBuilder(args);

//add services to the container

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

//configuire the http request pipeline

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

//before publishing
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
  var context = services.GetRequiredService<DataContext>();

  var userManager = services.GetRequiredService<UserManager<AppUser>>();
  var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

  // Apply any pending migrations
  await context.Database.MigrateAsync();

  // await Seed.SeedUsers(context);
  await Seed.SeedUsers(userManager, roleManager);

}
catch (Exception ex)
{
  var logger = services.GetRequiredService<ILogger<Program>>();
  logger.LogError(ex, "An Error occured during migration");
}

await app.RunAsync();

