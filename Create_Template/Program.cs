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
    // Cookie Ad�
    opts.Cookie.Name = "Template.auth";
    //Cookie g�ncelleme  S�resi
    opts.ExpireTimeSpan = TimeSpan.FromDays(7);
    //her giri�inde expire s�resininin s�f�rlanmas� sa�lar. s�f�rlamamas� i�in kapat�ld�
    opts.SlidingExpiration = false;
    //Cookie �al��mad���nda  yada s�resi doldu�unda ilerlemesi gereken sayfan�n pathi 
    opts.LoginPath = "/Account/Login";
    //kullan�c� logout oldu�undak� sayfan�n pathi
    opts.LogoutPath= "/Account/Logout";
    //Yetkisiz giri� durumunda y�nlendirilecek sayfa Pathi
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
#region CookieAut sisteminin �al��mas� i�in  bu methodunda pipeline eklenmi� olmas� gerekmekte
app.UseAuthentication(); 
#endregion

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
