using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Database Connection
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

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

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
      options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
      {
          Type = SecuritySchemeType.OAuth2,
          Flows = new OpenApiOAuthFlows
          {
              Implicit = new OpenApiOAuthFlow
              {
                  AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                  Scopes = new Dictionary<string, string>
                  {
                      ["readAccess"] = "Access read operations",
                      ["writeAccess"] = "Access write operations"
                  }
              }
          }
      })
   //options =>
   //{
   //    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
   //    {
   //        Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
   //                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
   //                    "Ejemplo: \"12345abcdef\"",
   //        Name = "Authorization",
   //        In = ParameterLocation.Header,
   //        Type = SecuritySchemeType.Http,
   //        Scheme = "Bearer"
   //    });
   //    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
   // {
   //   {
   //     new OpenApiSecurityScheme
   //     {
   //       Reference = new OpenApiReference
   //       {
   //         Type = ReferenceType.SecurityScheme,
   //         Id = "Bearer"
   //       },
   //       Scheme = "oauth2",
   //       Name = "Bearer",
   //       In = ParameterLocation.Header
   //     },
   //     new List<string>()
   //   }
   // });
   //}
);
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(PolicyName.AllowSpecificOrigin);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
