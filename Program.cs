using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
// Adicionar os serviços MVC
builder.Services.AddControllersWithViews();

// Configuração da Autenticação
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Autenticacao/LoginGoogle";
    options.AccessDeniedPath = "/Home/AcessoNegado"; // Rota caso alguém tente entrar na Sala do Trono sem ser Admin
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    
    // Intercetar o login para definir cargos (Roles)
   options.Events.OnCreatingTicket = context =>
    {
        var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
        var adminEmails = builder.Configuration.GetSection("Whitelist:Admins").Get<string[]>();

        if (!string.IsNullOrEmpty(email))
        {
            var emailLower = email.Trim().ToLower();

            // 1. VERIFICAÇÃO DA LISTA NEGRA (SISTEMA DE BANS)
            var pathListaNegra = Path.Combine(Directory.GetCurrentDirectory(), "ListaNegra.txt");
            if (File.Exists(pathListaNegra))
            {
                var bannedEmails = File.ReadAllLines(pathListaNegra);
                if (bannedEmails.Contains(emailLower))
                {
                    context.Fail("ACESSO NEGADO. O utilizador foi banido do Reino.");
                    return Task.CompletedTask;
                }
            }

            // NOVO: 2. REGISTRAR USUÁRIOS CADASTRADOS (VISITANTES)
            var pathCadastrados = Path.Combine(Directory.GetCurrentDirectory(), "UsuariosCadastrados.txt");
            List<string> cadastrados = File.Exists(pathCadastrados) ? File.ReadAllLines(pathCadastrados).ToList() : new List<string>();
            
            if (!cadastrados.Contains(emailLower))
            {
                File.AppendAllText(pathCadastrados, emailLower + Environment.NewLine);
            }

            // 3. VERIFICAÇÃO DO ADMIN (WHITELIST)
            if (adminEmails != null && adminEmails.Contains(email))
            {
                var identity = (ClaimsIdentity)context.Principal.Identity;
                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            }
        }

        return Task.CompletedTask;
    };
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60); // A sessão do Trono expira em 60 min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection(); // Redireciona tudo para HTTPS (segurança)
app.UseStaticFiles();      

app.UseRouting();
app.UseSession();
// É OBRIGATÓRIO ter estas duas linhas por esta ordem antes do app.MapControllerRoute
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();