using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using WebInterfaceDemoGRPC.Interfaces;
using WebInterfaceDemoGRPC.Mapper;
using WebInterfaceDemoGRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(MapperProfiler));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

builder.Services.AddGrpcClient<ProductProtoService.ProductProtoServiceClient>(opt =>
    opt.Address = new Uri(builder.Configuration["MainService:ProductServerUrl"])
);
builder.Services.AddGrpcClient<ShoppingCartProtoService.ShoppingCartProtoServiceClient>(opt =>
    opt.Address = new Uri(builder.Configuration["MainService:ShoppingCartServerUrl"])
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();