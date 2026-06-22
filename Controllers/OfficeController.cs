using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SeuProjeto.Controllers
{
    public class OfficeController : Controller
    {
        private string BaseDrivePath => Path.Combine(Directory.GetCurrentDirectory(), "StangeTreasure_Drive");

        private string ObterCaminhoSeguro(string pathAtual, string nomeArquivo)
        {
            var caminho = Path.Combine(BaseDrivePath, pathAtual ?? "", nomeArquivo ?? "");
            var caminhoReal = Path.GetFullPath(caminho);
            if (!caminhoReal.StartsWith(Path.GetFullPath(BaseDrivePath))) throw new UnauthorizedAccessException();
            return caminhoReal;
        }

        // 1. O Editor Visual (A tela que você vê)
        [Authorize(Roles = "Admin")]
        public IActionResult Editor(string pathAtual, string nomeArquivo)
        {
            ViewBag.PathAtual = pathAtual ?? "";
            ViewBag.NomeArquivo = nomeArquivo;
            ViewBag.Extensao = Path.GetExtension(nomeArquivo).Replace(".", "").ToLower();
            
            // Define se é documento de texto, planilha ou apresentação
            ViewBag.DocumentType = ViewBag.Extensao switch {
                "xlsx" or "xls" or "csv" => "cell",
                "pptx" or "ppt" => "slide",
                _ => "word"
            };

            // NOVO: Gera a chave única exigida pelo OnlyOffice para co-edição
            var caminhoFisico = ObterCaminhoSeguro(pathAtual, nomeArquivo);
            var fileInfo = new System.IO.FileInfo(caminhoFisico);
            // Cria um Hash combinando o nome do ficheiro e a data da última modificação
            ViewBag.DocumentKey = Math.Abs((nomeArquivo + fileInfo.LastWriteTime.Ticks).GetHashCode()).ToString();

            return View();
        }

        // 2. A Rota de Download (Onde o Docker vem buscar o arquivo)
        [AllowAnonymous] // Tem que ser anónimo para o Docker conseguir puxar
        public IActionResult Download(string pathAtual, string nomeArquivo)
        {
            var caminho = ObterCaminhoSeguro(pathAtual, nomeArquivo);
            if (!System.IO.File.Exists(caminho)) return NotFound();
            return PhysicalFile(caminho, "application/octet-stream");
        }

       // 3. O Callback do WOPI (Onde o Docker devolve o arquivo salvo)
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken] // <-- O PASSE VIP DE SEGURANÇA
        public async Task<IActionResult> Callback(string pathAtual, string nomeArquivo)
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                // Garante que o C# não implique com letras maiúsculas/minúsculas no JSON
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<OnlyOfficeCallback>(body, options);

                // Status 2 (Pronto) ou Status 6 (Forçar Salvar)
                if (data != null && (data.status == 2 || data.status == 6))
                {
                    var caminho = ObterCaminhoSeguro(pathAtual, nomeArquivo);
                    
                    using var httpClient = new HttpClient();
                    var fileBytes = await httpClient.GetByteArrayAsync(data.url);
                    
                    // Salva por cima do arquivo antigo
                    await System.IO.File.WriteAllBytesAsync(caminho, fileBytes);
                }

                // O OnlyOffice EXIGE que a resposta seja exatamente {"error": 0}
                return Json(new { error = 0 });
            }
            catch (Exception ex)
            {
                // Se o erro for no C#, ele imprime no terminal do servidor para podermos investigar!
                Console.WriteLine($"\n[ONLYOFFICE_ERROR] Falha ao gravar {nomeArquivo}: {ex.Message}\n");
                return Json(new { error = 1 }); 
            }
        }

        // Classe auxiliar para ler o JSON do OnlyOffice
        public class OnlyOfficeCallback {
            public int status { get; set; }
            public string url { get; set; }
        }
    }
}