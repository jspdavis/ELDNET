using JobPortal.Data;
using JobPortal.Services;
using JobPortal.Settings;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Session ───────────────────────────────────────────────────────────────────
// Session requires a distributed memory cache as its backing store
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Session expires after 60 minutes of inactivity
    options.IdleTimeout     = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;   // Prevents JS access to session cookie
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

// ── Database & Services (Dependency Injection) ────────────────────────────────
// DatabaseContext is a singleton because it only holds the connection string
builder.Services.AddSingleton<DatabaseContext>();

// Services are scoped — one instance per HTTP request
builder.Services.AddScoped<IUserService,        UserService>();
builder.Services.AddScoped<IJobService,         JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAdminService,       AdminService>();
builder.Services.AddScoped<IProfileService,     ProfileService>();

// ── File Storage ──────────────────────────────────────────────────────────────
// Resolve the App_Data path relative to the content root and ensure both
// upload subdirectories exist before the app starts accepting requests.
var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(Path.Combine(appDataPath, "resumes"));
Directory.CreateDirectory(Path.Combine(appDataPath, "cover_letters"));
builder.Services.AddSingleton(new FileStorageSettings { AppDataPath = appDataPath });

// ── HttpContextAccessor ───────────────────────────────────────────────────────
// Needed so Razor views can access HttpContext.Session
builder.Services.AddHttpContextAccessor();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Error Handling ────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseStaticFiles();    // Serve wwwroot files (CSS, JS, images)

app.UseRouting();

// Session must be registered BEFORE authorization and after routing
app.UseSession();

app.UseAuthorization();

// ── Default Route ─────────────────────────────────────────────────────────────
// Maps /Controller/Action/{id?} to the corresponding controller action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
