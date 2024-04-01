using Cache.WebAPI.Context;
using Cache.WebAPI.Models;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicaitonDbContext>(options =>
{
    options.UseInMemoryDatabase("MyDb");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var scoped = builder.Services.BuildServiceProvider();
ApplicaitonDbContext context = scoped.GetRequiredService<ApplicaitonDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("SeedData", () =>
{
    List<Product> products = new List<Product>();

    for (int i = 0; i < 1000; i++)
    {
        Product product = new()
        {
            Name = "Product " + i
        };
        products.Add(product);
    }
    context.Products.AddRange(products);
    context.SaveChanges();

    return new { Message = "Product SeedData is success" };
});

app.MapGet("GetAllProducts", async(int pageNumber = 1, int pageSize = 10, CancellationToken cancellation = default) =>
{
    List<Product> products = await context.Products.Skip(pageSize* pageNumber).Take(pageSize).ToListAsync(cancellation);
  
    decimal count = await context.Products.CountAsync(cancellation);
    decimal totalPageNumbers = Math.Ceiling(count / pageSize);
    bool isFirstPage = pageNumber == 1 ? true : false;
    bool isLastPage = pageNumber == totalPageNumbers ? true : false;

    var response = new
    {
        Data = products,
        Count = count,
        TotalPageNumbers = totalPageNumbers,
        IsFirstPage = isFirstPage,
        IsLastPage = isLastPage,
        pageNumber = pageNumber,
        pageSize = pageSize
    };

    return response;
});

app.Run();

