using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeuProjeto.Controllers
{
    [Authorize] 
    public class BibliotecaController : Controller
    {
        private string CaminhoWiki => Path.Combine(Directory.GetCurrentDirectory(), "WikiContent.html");
        
        // Caminho do seu Drive
        private string BaseDrivePath => Path.Combine(Directory.GetCurrentDirectory(), "StangeTreasure_Drive");

        public IActionResult Index()
        {
            if (System.IO.File.Exists(CaminhoWiki))
            {
                ViewBag.WikiContent = System.IO.File.ReadAllText(CaminhoWiki);
            }
            else
            {
                ViewBag.WikiContent = "<section id='introducao' class='wiki-section'><h1 class='wiki-title'>Wiki Vazia</h1><p>Clique em [ MODO EDIÇÃO ] para começar a escrever.</p></section>";
            }

            // Libera os poderes de Mestre
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View();
        }

        // ===============================================
        // NOVAS ROTAS DO EDITOR MÁGICO (API INLINE)
        // ===============================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult SalvarInline([FromBody] WikiData data)
        {
            try
            {
                System.IO.File.WriteAllText(CaminhoWiki, data.Conteudo ?? "");
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ObterImagensTreasure()
        {
            if (!Directory.Exists(BaseDrivePath)) return Json(new List<object>());

            var extensoes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            
            var imagens = Directory.GetFiles(BaseDrivePath, "*.*", SearchOption.AllDirectories)
                .Where(f => extensoes.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => {
                    var dir = Path.GetDirectoryName(f) ?? BaseDrivePath;
                    var relPath = Path.GetRelativePath(BaseDrivePath, dir);
                    var pathAtual = relPath == "." ? "" : relPath;
                    var nomeArquivo = Path.GetFileName(f);

                    return new {
                        nome = nomeArquivo,
                        pasta = pathAtual == "" ? "Raiz" : pathAtual.Replace("\\", "/"),
                        
                        // CORREÇÃO: Agora aponta para a nova rota pública da Biblioteca
                        url = Url.Action("VerImagem", "Biblioteca", new { pasta = pathAtual, arquivo = nomeArquivo })
                    };
                }).ToList();

            return Json(imagens);
        }

        // NOVO: Rota pública para carregar imagens na Wiki sem precisar da senha do Trono
        [HttpGet]
        [AllowAnonymous] // Permite que os jogadores leiam as imagens!
        public IActionResult VerImagem(string pasta, string arquivo)
        {
            var caminhoCompleto = Path.Combine(BaseDrivePath, pasta ?? "", arquivo ?? "");
            
            // Segurança Anti-Hacker: Impede ataques de "Directory Traversal" (ex: ../../etc/passwd)
            var caminhoReal = Path.GetFullPath(caminhoCompleto);
            var baseReal = Path.GetFullPath(BaseDrivePath);
            if (!caminhoReal.StartsWith(baseReal)) return Unauthorized();

            if (System.IO.File.Exists(caminhoReal))
            {
                string ext = Path.GetExtension(caminhoReal).ToLower();
                string mimeType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                return PhysicalFile(caminhoReal, mimeType);
            }
            return NotFound();
        }
    }

    public class WikiData 
    { 
        public string Conteudo { get; set; } 
    }
}