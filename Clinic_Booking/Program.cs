using Clinic_Booking.Data;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Configuration;
using Clinic_Booking.IServices.IAnalyticsServices;
using Clinic_Booking.IServices.IAppReleaseServices;
using Clinic_Booking.IServices.IAppVersionServices;
using Clinic_Booking.IServices.IAppointmentServices;
using Clinic_Booking.IServices.IAppointmentReschedulingServices;
using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.IClinicServices;
using Clinic_Booking.IServices.IClinicExceptionServices;
using Clinic_Booking.IServices.IDayServices;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Clinic_Booking.IServices.IDoctorFeatureServices;
using Clinic_Booking.IServices.IDoctorOfferServices;
using Clinic_Booking.IServices.IDoctorServices;
using Clinic_Booking.IServices.IDoctorSubscriptionServices;
using Clinic_Booking.IServices.IFeatureServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.IServices.IReviewServices;
using Clinic_Booking.IServices.ISpecializationServices;
using Clinic_Booking.IServices.ISubscriptionPackagesServices;
using Clinic_Booking.IServices.IUserServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Clinic_Booking.IServices.INotificationDeliveryHelper;
using Clinic_Booking.IServices.IMessageServices;
using Clinic_Booking.Services.AnalyticsServices;
using Clinic_Booking.Services.AppointmentReminderServices;
using Clinic_Booking.Services.AppointmentReschedulingServices;
using Clinic_Booking.Services.AppointmentServices;
using Clinic_Booking.Services.AppReleaseServices;
using Clinic_Booking.Services.AppVersionServices;
using Clinic_Booking.Services.BookingSmsServices;
using Clinic_Booking.Services.ClinicServices;
using Clinic_Booking.Services.ClinicExceptionServices;
using Clinic_Booking.Services.DayServices;
using Clinic_Booking.Services.DoctorAvailabilityServices;
using Clinic_Booking.Services.DoctorFeatureServices;
using Clinic_Booking.Services.DoctorOfferServices;
using Clinic_Booking.Services.DoctorServices;
using Clinic_Booking.Services.DoctorSubscriptionServices;
using Clinic_Booking.Services.FeatureServices;
using Clinic_Booking.Services.LoadServices;
using Clinic_Booking.Services.MaintenanceServices;
using Clinic_Booking.Services.NotificationDeliveryRetryServices;
using Clinic_Booking.Services.PushNotificationServices;
using Clinic_Booking.Services.ReviewServices;
using Clinic_Booking.Services.SpecializationServices;
using Clinic_Booking.Services.SubscriptionPackagesServices;
using Clinic_Booking.Services.SubscriptionExpirationServices;
using Clinic_Booking.Services.UserServices;
using Clinic_Booking.Services.WhatsAppMessageServices;
using Clinic_Booking.Services.NotificationDeliveryHelper;
using Clinic_Booking.Hubs;
using Clinic_Booking.Services.MessageServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IAnalyticsServices, AnalyticsServices>();
builder.Services.AddScoped<ILoadServices, LoadServices>();
builder.Services.AddScoped<ISubscriptionPackagesServices, SubscriptionPackagesServices>();
builder.Services.AddScoped<IDayServices, DayServices>();
builder.Services.AddScoped<ISpecializationServices, SpecializationServices>();
builder.Services.AddScoped<IDoctorServices, DoctorServices>();
builder.Services.AddScoped<IDoctorSubscriptionServices, DoctorSubscriptionServices>();
builder.Services.AddScoped<IFeatureServices, FeatureServices>();
builder.Services.AddScoped<IDoctorFeatureServices, DoctorFeatureServices>();
builder.Services.AddScoped<IDoctorOfferServices, DoctorOfferServices>();
builder.Services.AddScoped<IDoctorAvailabilityServices, DoctorAvailabilityService>();
builder.Services.AddScoped<INotificationDeliveryHelper, NotificationDeliveryHelper>();
builder.Services.AddScoped<IAppointmentServices, AppointmentServices>();
builder.Services.AddScoped<IAppointmentReschedulingServices, AppointmentReschedulingServices>();
builder.Services.AddScoped<IAppReleaseServices, AppReleaseServices>();
builder.Services.AddScoped<IAppVersionServices, AppVersionServices>();
builder.Services.AddScoped<IBookingSmsServices, DevelopmentBookingSmsServices>();
builder.Services.AddScoped<IClinicServices, ClinicServices>();
builder.Services.AddScoped<IClinicExceptionServices, ClinicExceptionServices>();
builder.Services.AddScoped<IReviewServices, ReviewServices>();
builder.Services.AddScoped<IMessageServices, MessageServices>();
builder.Services.AddSingleton<OnlineUserTracker>();
builder.Services.AddSignalR();
builder.Services.AddHttpClient<IPushNotificationServices, PushNotificationServices>();
builder.Services.AddHttpClient<IWhatsAppMessageServices, WhatsAppMessageServices>();
builder.Services.AddHostedService<SubscriptionExpirationService>();
builder.Services.AddHostedService<AppointmentReminderService>();
builder.Services.AddHostedService<DataCleanupService>();
builder.Services.AddHostedService<NotificationDeliveryRetryService>();
builder.Services.Configure<BookingOtpOptions>(
    builder.Configuration.GetSection(BookingOtpOptions.SectionName));
builder.Services.Configure<PushNotificationOptions>(
    builder.Configuration.GetSection(PushNotificationOptions.SectionName));
builder.Services.Configure<WhatsAppMessageOptions>(
    builder.Configuration.GetSection(WhatsAppMessageOptions.SectionName));
builder.Services.Configure<AppointmentReminderOptions>(
    builder.Configuration.GetSection(AppointmentReminderOptions.SectionName));





builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("Auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("AccountRecovery", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0
            }));

    options.AddPolicy("Otp", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 6,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));

    options.AddPolicy("Booking", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));

    options.AddPolicy("Messaging", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                httpContext.Connection.RemoteIpAddress?.ToString() ??
                "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<AspNetUsers, AspNetRoles>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
    };
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Clinic Booking API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15); // ⏰ صلاحية التوكن 15 دقيقة
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<Clinic_Booking.Middleware.ExceptionMiddleware>();

//if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/hubs/message");

// Only serve AppDownloads publicly; image folders are secured via /api/files/*
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "AppDownloads")),
    RequestPath = "/AppDownloads"
});

//app.Use(async (context, next) =>
//{
//context.Response.Headers["Content-Security-Policy"] =
//    "default-src 'self'; " +
//    "script-src 'self'; " +
//    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
//    "style-src-elem 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
//    "font-src 'self' https://fonts.gstatic.com data:; " +
//    "img-src 'self' data:; " +
//    "connect-src 'self' http://localhost:* https://*; " +
//    "form-action 'self'; " +
//    "base-uri 'self'; " +
//    "frame-ancestors 'none';";

//    await next();
//});

if (app.Configuration.GetValue("Database:AutoMigrate", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    await AppVersionPolicySeeder.SeedAsync(dbContext);
}

app.Run();
