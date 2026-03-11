using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.Interface;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
