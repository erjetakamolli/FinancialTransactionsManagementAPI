using Microsoft.EntityFrameworkCore;
using FinancialTransactionsManagementAPI.Data;
using FinancialTransactionsManagementAPI.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the repository
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinancialTransactionsAPI", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinancialTransactionsAPI v1");
});

app.UseAuthorization();
app.MapControllers();
app.Run();
