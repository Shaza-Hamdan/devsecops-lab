using Registration.Persistence.Repository;
using Registration.Services;
using Microsoft.EntityFrameworkCore;
using Registration.Services.Implementations;
using ResFunctions;
using Registration.Persistence.entity;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Registration.Persistence.Repository;
using GitFile.FileCreate;
using Services.Interfaces;
using Services.Implementations;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("MyConnection");

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddSingleton<RegistrationFunctions>();
builder.Services.AddControllers();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IGitService, GitService>();
builder.Services.AddScoped<IRepositoryService, RepositoryService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings.GetValue<string>("SecretKey");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API V1", Version = "v1" });

    // start Add JWT Authentication support in Swagger(authorization)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });//end authorization
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1"); });
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

// Add authentication and authorization middleware
app.UseAuthentication(); // Ensures the request has a valid token
app.UseAuthorization();  // Ensures the authenticated user has proper roles/permissions

// Add role-based middleware
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/admin"),
    appBuilder => appBuilder.Use(async (context, next) =>
    {
        var middleware = new RoleAuthorizationMiddleware(next, new[] { "Admin" });
        await middleware.InvokeAsync(context);
    })
);

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/user"),
    appBuilder => appBuilder.Use(async (context, next) =>
    {
        var middleware = new RoleAuthorizationMiddleware(next, new[] { "User" });
        await middleware.InvokeAsync(context);
    })
);

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/guest"),
    appBuilder => appBuilder.Use(async (context, next) =>
    {
        var middleware = new RoleAuthorizationMiddleware(next, new[] { "Guest" });
        await middleware.InvokeAsync(context);
    })
);


app.MapControllers();

await CreateFirstAdmin(app);

app.Run();

// Method to create the first admin user if no users exist
async Task CreateFirstAdmin(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDBContext>();
        var registrationFunctions = services.GetRequiredService<RegistrationFunctions>();

        // Check if there are any users in the database
        if (context.registrations.Any())
        {
            context.registrations.RemoveRange(context.registrations);

            context.Roles.RemoveRange(context.Roles);

            await context.SaveChangesAsync();
        }
        // Create the roles 
        var adminRole = new RoleEntity { Name = "Admin" };
        var userRole = new RoleEntity { Name = "User" };
        var guestRole = new RoleEntity { Name = "Guest" };
        context.Roles.AddRange(adminRole, userRole, guestRole);
        await context.SaveChangesAsync();

        // Generate salt for the admin user
        string adminSalt = registrationFunctions.GenerateSalt();
        if (string.IsNullOrEmpty(adminSalt))
        {
            throw new Exception("Salt generation failed for admin user.");
        }

        // Hash the admin password using Stribog
        string adminPasswordHash = registrationFunctions.HashPasswordWithStribog("adminpassword", adminSalt);

        // Create the admin user
        var adminUser = new RegistrationUser
        {
            UserName = "admin",
            Email = "admin@mail.com",
            PasswordHash = adminPasswordHash,
            PhoneNumber = "1234567890",
            roleId = adminRole.Id,
            Salt = adminSalt
        };

        context.registrations.Add(adminUser);
        await context.SaveChangesAsync();
    }
}


string GenerateSalt()
{
    var saltBytes = new byte[16];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(saltBytes);
    }
    return Convert.ToBase64String(saltBytes);
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
