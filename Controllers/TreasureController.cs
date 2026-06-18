using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Compression;

namespace SeuProjeto.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TreasureController : Controller
    {
        // Define onde o Drive vai existir (FORA da pasta wwwroot por segurança)
        private string BaseDrivePath => Path.Combine(Directory.GetCurrentDirectory(), "StangeTreasure_Drive");

        public TreasureController()
        {
            // Cria a pasta master do Drive na primeira vez que o sistema iniciar
            if (!Directory.Exists(BaseDrivePath))
            {
                Directory.CreateDirectory(BaseDrivePath);
            }
        }

        // Verifica se a sessão do Trono está ativa
        private bool TronoBloqueado() => HttpContext.Session.GetString("TronoDesbloqueado") != "sim";

        // Previne que alguém tente usar "../" para aceder a ficheiros do Windows/Linux
        private string ObterCaminhoSeguro(string subCaminho)
        {
            if (string.IsNullOrEmpty(subCaminho)) return BaseDrivePath;
            var caminhoCompleto = Path.GetFullPath(Path.Combine(BaseDrivePath, subCaminho));
            if (!caminhoCompleto.StartsWith(Path.GetFullPath(BaseDrivePath))) throw new UnauthorizedAccessException("Tentativa de invasão detetada.");
            return caminhoCompleto;
        }

        // NOVO: Lê todas as pastas e subpastas para criar os menus Dropdown
        private List<string> ObterTodasPastas()
        {
            var lista = new List<string> { "" }; // O espaço em branco representa a pasta Raiz
            if (Directory.Exists(BaseDrivePath))
            {
                var dirs = Directory.GetDirectories(BaseDrivePath, "*", SearchOption.AllDirectories);
                foreach (var dir in dirs)
                {
                    lista.Add(System.IO.Path.GetRelativePath(BaseDrivePath, dir));
                }
            }
            return lista;
        }

        // 1. PÁGINA PRINCIPAL DO DRIVE
        public IActionResult Index(string path = "")
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");

            var caminhoAtual = ObterCaminhoSeguro(path);
            if (!Directory.Exists(caminhoAtual)) return RedirectToAction("Index");

            ViewBag.CaminhoAtual = path;
            ViewBag.Pastas = Directory.GetDirectories(caminhoAtual).Select(System.IO.Path.GetFileName).ToList();
            ViewBag.Arquivos = Directory.GetFiles(caminhoAtual).Select(System.IO.Path.GetFileName).ToList();
            ViewBag.CaminhoPai = string.IsNullOrEmpty(path) ? "" : System.IO.Path.GetDirectoryName(path);
            ViewBag.Backlog = System.IO.File.Exists(CaminhoBacklog) ? System.IO.File.ReadAllText(CaminhoBacklog) : "";
            
            // NOVO: Envia todas as pastas existentes para os menus
            ViewBag.TodasPastas = ObterTodasPastas();
            // NOVO: Envia mensagem de erro caso tente mover uma pasta para dentro dela mesma
            if (TempData["ErroDrive"] != null) ViewBag.Erro = TempData["ErroDrive"];

            return View();
        }

        // 2. CRIAR PASTA
        [HttpPost]
        public IActionResult CriarPasta(string pathAtual, string nomePasta)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");
            if (!string.IsNullOrWhiteSpace(nomePasta))
            {
                var novaPasta = ObterCaminhoSeguro(Path.Combine(pathAtual ?? "", nomePasta));
                if (!Directory.Exists(novaPasta)) Directory.CreateDirectory(novaPasta);
            }
            return RedirectToAction("Index", new { path = pathAtual });
        }

        // 3. UPLOAD DE FICHEIRO (Atualizado para receber a pasta de destino)
        [HttpPost]
        public async Task<IActionResult> Upload(string pathAtual, string destinoUpload, List<IFormFile> arquivosUpload)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");
            
            if (arquivosUpload != null && arquivosUpload.Count > 0)
            {
                foreach (var arquivo in arquivosUpload)
                {
                    if (arquivo.Length > 0)
                    {
                        var destino = ObterCaminhoSeguro(System.IO.Path.Combine(destinoUpload ?? "", arquivo.FileName));
                        using (var stream = new FileStream(destino, FileMode.Create))
                        {
                            await arquivo.CopyToAsync(stream);
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { path = pathAtual });
        }

        // 4. ELIMINAR (Ficheiro ou Pasta)
        [HttpPost]
        public IActionResult Deletar(string pathAtual, string nomeItem, bool isPasta)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");
            var alvo = ObterCaminhoSeguro(Path.Combine(pathAtual ?? "", nomeItem));

            if (isPasta && Directory.Exists(alvo)) Directory.Delete(alvo, true); // Apaga pasta e tudo lá dentro
            else if (!isPasta && System.IO.File.Exists(alvo)) System.IO.File.Delete(alvo);

            return RedirectToAction("Index", new { path = pathAtual });
        }

        // 5. DOWNLOAD E VISUALIZAÇÃO DE IMAGENS
        public IActionResult ServirArquivo(string pathAtual, string nomeArquivo, bool download = false)
        {
            if (TronoBloqueado()) return Unauthorized();
            var alvo = ObterCaminhoSeguro(Path.Combine(pathAtual ?? "", nomeArquivo));
            if (!System.IO.File.Exists(alvo)) return NotFound();

            var extensao = Path.GetExtension(alvo).ToLower();
            string contentType = "application/octet-stream"; // Padrão para download
            
            // Se não for download forçado e for imagem, diz ao browser para mostrar na tela
            if (!download)
            {
                if (extensao == ".jpg" || extensao == ".jpeg") contentType = "image/jpeg";
                else if (extensao == ".png") contentType = "image/png";
                else if (extensao == ".gif") contentType = "image/gif";
                else if (extensao == ".txt") contentType = "text/plain";
            }

            // PhysicalFile envia o ficheiro com segurança, mesmo fora da wwwroot
            return PhysicalFile(alvo, contentType, download ? nomeArquivo : null);
        }

        // 6. EDITOR DE TEXTO (GET)
        public IActionResult Editar(string pathAtual, string nomeArquivo)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");
            var alvo = ObterCaminhoSeguro(Path.Combine(pathAtual ?? "", nomeArquivo));
            
            if (!System.IO.File.Exists(alvo)) return RedirectToAction("Index", new { path = pathAtual });

            ViewBag.CaminhoAtual = pathAtual;
            ViewBag.NomeArquivo = nomeArquivo;
            ViewBag.Conteudo = System.IO.File.ReadAllText(alvo);
            
            return View();
        }

        // 7. SALVAR TEXTO EDITADO (POST)
        [HttpPost]
        public IActionResult SalvarEdicao(string pathAtual, string nomeArquivo, string conteudo)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");
            var alvo = ObterCaminhoSeguro(Path.Combine(pathAtual ?? "", nomeArquivo));
            
            System.IO.File.WriteAllText(alvo, conteudo ?? "");
            return RedirectToAction("Index", new { path = pathAtual });
        }

        // NOVO: 8. MOVER ITENS (Ficheiros ou Pastas)
        [HttpPost]
        public IActionResult Mover(string pathAtual, string nomeItem, string destinoItem)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");

            try
            {
                var origem = ObterCaminhoSeguro(System.IO.Path.Combine(pathAtual ?? "", nomeItem));
                var destino = ObterCaminhoSeguro(System.IO.Path.Combine(destinoItem ?? "", nomeItem));

                if (origem != destino)
                {
                    if (System.IO.File.Exists(origem)) System.IO.File.Move(origem, destino);
                    else if (Directory.Exists(origem)) Directory.Move(origem, destino);
                }
            }
            catch (Exception ex)
            {
                // Proteção caso tente mover uma pasta principal para dentro de uma subpasta dela mesma
                TempData["ErroDrive"] = "Erro ao mover: Operação inválida.";
            }

            return RedirectToAction("Index", new { path = pathAtual });
        }

        // 9. Lê e Salva o Backlog
        private string CaminhoBacklog => System.IO.Path.Combine(BaseDrivePath, ".backlog.txt");

        [HttpPost]
        public IActionResult SalvarBacklog(string pathAtual, string conteudoBacklog)
        {
            if (TronoBloqueado()) return RedirectToAction("Portao", "Trono");
            System.IO.File.WriteAllText(CaminhoBacklog, conteudoBacklog ?? "");
            return RedirectToAction("Index", new { path = pathAtual });
        }

        // 10. BACKUP DO DRIVE COMPLETO
        [HttpGet]
        public IActionResult FazerBackup()
        {
            if (TronoBloqueado()) return Unauthorized();

            string tempZipPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Backup_Temporario.zip");
            
            // Se já existir um zip antigo, apaga
            if (System.IO.File.Exists(tempZipPath)) System.IO.File.Delete(tempZipPath);

            // Zipa a pasta inteira do Stange Treasure
            ZipFile.CreateFromDirectory(BaseDrivePath, tempZipPath);

            // Lê o arquivo para a memória e apaga o zip temporário do servidor
            byte[] fileBytes = System.IO.File.ReadAllBytes(tempZipPath);
            System.IO.File.Delete(tempZipPath);

            return File(fileBytes, "application/zip", $"StangeTreasure_Backup_{DateTime.Now:yyyyMMdd_HHmm}.zip");
        }
    }
}