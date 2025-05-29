using Clinic_Booking.Data;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.User;
using Clinic_Booking.IServices.IDayServices;
using Clinic_Booking.IServices.IDoctorFeatureServices;
using Clinic_Booking.IServices.IDoctorServices;
using Clinic_Booking.IServices.IDoctorSubscriptionServices;
using Clinic_Booking.IServices.IEmailServices;
using Clinic_Booking.IServices.IFeatureServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.ISpecializationServices;
using Clinic_Booking.IServices.ISubscriptionPackagesServices;
using Clinic_Booking.IServices.IUserServices;
using Clinic_Booking.Services.DayServices;
using Clinic_Booking.Services.DoctorFeatureServices;
using Clinic_Booking.Services.DoctorServices;
using Clinic_Booking.Services.DoctorSubscriptionServices;
using Clinic_Booking.Services.EmailServices;
using Clinic_Booking.Services.FeatureServices;
using Clinic_Booking.Services.LoadServices;
using Clinic_Booking.Services.SpecializationServices;
using Clinic_Booking.Services.SubscriptionPackagesServices;
using Clinic_Booking.Services.UserServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<ILoadServices, LoadServices>();
builder.Services.AddScoped<ISubscriptionPackagesServices, SubscriptionPackagesServices>();
builder.Services.AddScoped<IDayServices, DayServices>();
builder.Services.AddScoped<ISpecializationServices, SpecializationServices>();
builder.Services.AddScoped<IDoctorServices, DoctorServices>();
builder.Services.AddScoped<IDoctorSubscriptionServices, DoctorSubscriptionServices>();
builder.Services.AddScoped<IFeatureServices, FeatureServices>();
builder.Services.AddScoped<IDoctorFeatureServices, DoctorFeatureServices>();


builder.Services.AddTransient<IEmailServices, EmailServices>();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
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
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Blood Bank", Version = "v1" });
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
    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15); // ⏰ صلاحية التوكن 15 دقيقة
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(MyAllowSpecificOrigins);

app.UseStaticFiles();

var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

dbContext.Database.Migrate();

app.Run();
