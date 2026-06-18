using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeuProjeto.Controllers
{
    [Authorize] 
    public class BibliotecaController : Controller
    {
        // Define o caminho onde a lore ficará guardada no servidor
        private string CaminhoWiki => Path.Combine(Directory.GetCurrentDirectory(), "WikiContent.html");

        public IActionResult Index()
        {
            // Se o ficheiro existir, lê o conteúdo. Se não, mostra uma mensagem padrão.
            if (System.IO.File.Exists(CaminhoWiki))
            {
                ViewBag.WikiContent = System.IO.File.ReadAllText(CaminhoWiki);
            }
            else
            {
                ViewBag.WikiContent = "<section id='introducao' class='wiki-section'><h1 class='wiki-title'>Wiki Vazia</h1><p>Vá à Sala do Trono para escrever os primeiros pergaminhos.</p></section>";
            }

            return View();
        }
    }
}