using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace SeuProjeto.Controllers
{
    public class AutenticacaoController : Controller
    {
        // Acionado quando clica em [Login Google]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties 
            { 
                // Após o login com sucesso, volta para o Salão Principal
                RedirectUri = Url.Action("Index", "Home") 
            };
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Acionado quando clica em [Logout]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}