using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StangeCastle.Data;

// Apenas UM builder no projeto inteiro!
var builder = WebApplication.CreateBuilder(args);

// 1. Configura o Banco de Dados (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Configura os Guardas (Identity Core puro, para telas customizadas)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Avisa aos Guardas para onde mandar os invasores (Nossas futuras telas)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Redireciona para cá se não estiver logado
    options.AccessDeniedPath = "/Auth/AcessoNegado"; // Redireciona para cá se não for Admin
});

// Adiciona o suporte ao MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurações do túnel HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. Liga a fiscalização (Autenticação e Autorização)
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();