using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using pahrmacy.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<pahrmacyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("pahrmacyContext") ?? throw new InvalidOperationException("Connection string 'pahrmacyContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// add session 
builder.Services.AddSession(options =>{options.IdleTimeout = TimeSpan.FromMinutes(20);});


builder.Services.AddCors(o =>
{
    o.AddPolicy("_myAllowSpecificOrigins", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


// Add session
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=useraccounts}/{action=Login}/{id?}");

app.UseCors("_myAllowSpecificOrigins");
app.UseHttpsRedirection();
app.MapControllers();


app.Run();
