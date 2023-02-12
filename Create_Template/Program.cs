using Create_Template.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DatabaseContext>(Opts =>
{
    Opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    //Opts.UseLazyLoadingProxies();
});

#region CookieSettings
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme.ToString()).AddCookie(opts =>
{
    // Cookie Adý
    opts.Cookie.Name = "Template.auth";
    //Cookie güncelleme  Süresi
    opts.ExpireTimeSpan = TimeSpan.FromDays(7);
    //her giriþinde expire süresininin sýfýrlanmasý saðlar. sýfýrlamamasý için kapatýldý
    opts.SlidingExpiration = false;
    //Cookie çalýþmadýðýnda  yada süresi dolduðunda ilerlemesi gereken sayfanýn pathi 
    opts.LoginPath = "/Account/Login";
    //kullanýcý logout olduðundakþ sayfanýn pathi
    opts.LogoutPath= "/Account/Logout";
    //Yetkisiz giriþ durumunda yönlendirilecek sayfa Pathi
    opts.AccessDeniedPath = "/Home/AccessDenied";




});

#endregion




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
#region CookieAut sisteminin çalýþmasý için  bu methodunda pipeline eklenmiþ olmasý gerekmekte
app.UseAuthentication(); 
#endregion

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
