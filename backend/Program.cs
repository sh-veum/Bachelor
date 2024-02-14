using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Netbackend.Services;
using NetBackend.Constants;
using NetBackend.Data.DbContexts;
using NetBackend.GraphQL;
using NetBackend.GraphQL.Mutations;
using NetBackend.Models.User;
using NetBackend.Services;
using NetBackend.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });

    // Define the Bearer Authentication Scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    // Ensure every request is authorized using the defined scheme
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add authentication (modified from https://youtu.be/ORzt0lks2H4?si=kXqRl7VUO9r9BPbN)
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

// Add authorization
builder.Services.AddAuthorizationBuilder();

// Configure DbContext
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var customerOneConnectionString = builder.Configuration.GetConnectionString("CustomerOneConnection");
var customerTwoConnectionString = builder.Configuration.GetConnectionString("CustomerTwoConnection");

builder.Services.AddDbContext<MainDbContext>(options =>
    options.UseNpgsql(defaultConnectionString));

builder.Services.AddDbContext<CustomerOneDbContext>(options =>
    options.UseNpgsql(customerOneConnectionString));

builder.Services.AddDbContext<CustomerTwoDbContext>(options =>
    options.UseNpgsql(customerTwoConnectionString));

builder.Services.AddIdentityCore<UserModel>()
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<MainDbContext>()
    .AddApiEndpoints();

// Services
builder.Services.AddScoped<IDbContextService, DbContextService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<IApiService, ApiService>();

// CORS policy with the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      policy =>
                      {
                          policy.WithOrigins(FrontendConstants.FrontEndURL)
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services
    .AddGraphQLServer()
    // .RegisterDbContext<MainDbContext>(DbContextKind.Pooled)
    // .RegisterDbContext<CustomerOneDbContext>(DbContextKind.Pooled)
    // .RegisterDbContext<CustomerTwoDbContext>(DbContextKind.Pooled)
    .AddQueryType<Query>()
    .AddMutationType<ApiKeyMutation>();

var app = builder.Build();

app.MapIdentityApi<UserModel>();

// Apllying CORS policy
app.UseCors("AllowSpecificOrigin");

app.UseRouting();

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Automatic migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<MainDbContext>();

    dbContext.Database.Migrate();

    // Make sure the roles and admin user is created
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<UserModel>>();

        await ApplicationDbInitializer.SeedRoles(roleManager);
        // Admin user
        await ApplicationDbInitializer.EnsureUser(userManager, roleManager, SecretConstants.AdminEmail, SecretConstants.AdminPassword, RoleConstants.AdminRole);
        // Test user
        await ApplicationDbInitializer.EnsureUser(userManager, roleManager, SecretConstants.TestEmail, SecretConstants.TestPassword, RoleConstants.CustomerRole);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }

    var customerOneDbContext = services.GetRequiredService<CustomerOneDbContext>();
    customerOneDbContext.Database.Migrate();

    var customerTwoDbContext = services.GetRequiredService<CustomerTwoDbContext>();
    customerTwoDbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.¨
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGraphQL();

app.Run();
