using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.Interface;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Database Connection
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

//Cache Configuration
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; //1MB
    options.UseCaseSensitivePaths = true;
});

//Injecting Repository
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRespository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//Adding Mapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//SecretKey
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("SecretKey is not configured");
}

//Authentication and Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{ 
    options.RequireHttpsMetadata = false; //To desactive https (it's important to have this true in production)
    options.SaveToken = true; //it's going to save our Token in an authentication context
    options.TokenValidationParameters = new TokenValidationParameters //Define our token parameters
    {
        ValidateIssuerSigningKey = true, //It's going to validate our token and signingKey
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), //To validate UTF8 in token and it's setting our secretKey
        ValidateIssuer = false, //To validate our issuer 
        ValidateAudience = false //To validate users or customer 
    };
});

builder.Services.AddControllers(option =>
    {
        option.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);
        option.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Profile20);
    }    
);


var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
    /*option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));*/ //?api-version
});

apiVersioningBuilder.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV"; // v1, v2, v3
    option.SubstituteApiVersionInUrl = true; // api/v{version}/products
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });

    options.SwaggerDoc("v1", new OpenApiInfo
        { 
            Version = "v1",
            Title = "Api Ecommerce",
            Description = "API to manage products and users",
            TermsOfService = new Uri("http://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Elvin Mendez",
                Url = new Uri("http://portfolio.com/info")
            },
            License = new OpenApiLicense
            {
                Name = "use licence",
                Url = new Uri("http://example.com/licences")
            }
        });
        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "v2",
            Title = "Api Ecommerce v2",
            Description = "API to manage products and users",
            TermsOfService = new Uri("http://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Elvin Mendez",
                Url = new Uri("http://portfolio.com/info")
            },
            License = new OpenApiLicense
            {
                Name = "use licence",
                Url = new Uri("http://example.com/licences")
            }
        });

});


builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
   {
    options.AddPolicy(PolicyName.AllowSpecificOrigin,
        builder =>
        {
            builder.WithOrigins("*").AllowAnyMethod().AllowAnyMethod();
        });
    }
); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}

app.UseHttpsRedirection();

app.UseCors(PolicyName.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
