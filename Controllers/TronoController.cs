using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace SeuProjeto.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TronoController : Controller
    {
        private readonly IConfiguration _config;

        // Construtor para injetar as configurações
        public TronoController(IConfiguration config)
        {
            _config = config;
        }
        // Caminho dinâmico para o ficheiro de bans
        // Caminhos dinâmicos
        private string CaminhoListaNegra => Path.Combine(Directory.GetCurrentDirectory(), "ListaNegra.txt");
        private string CaminhoCadastrados => Path.Combine(Directory.GetCurrentDirectory(), "UsuariosCadastrados.txt"); // NOVO
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("TronoDesbloqueado") != "sim")
            {
                return RedirectToAction("Portao");
            }

            // Lê a lista negra
            ViewBag.BannedUsers = System.IO.File.Exists(CaminhoListaNegra) 
                ? System.IO.File.ReadAllLines(CaminhoListaNegra).Where(e => !string.IsNullOrWhiteSpace(e)).ToList() 
                : new List<string>();

            // Lê a lista de cadastrados
            ViewBag.RegisteredUsers = System.IO.File.Exists(CaminhoCadastrados)
                ? System.IO.File.ReadAllLines(CaminhoCadastrados).Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
                : new List<string>();

            return View();
        }

        [HttpPost]
        public IActionResult BanirUsuario(string emailBanir)
        {
            if (!string.IsNullOrWhiteSpace(emailBanir))
            {
                // Adiciona o email ao ficheiro de texto, com quebra de linha
                System.IO.File.AppendAllText(CaminhoListaNegra, emailBanir.Trim().ToLower() + Environment.NewLine);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DesbanirUsuario(string emailDesbanir)
        {
            if (System.IO.File.Exists(CaminhoListaNegra))
            {
                // Lê todos, remove o selecionado e reescreve o ficheiro
                var linhas = System.IO.File.ReadAllLines(CaminhoListaNegra);
                var novaLista = linhas.Where(l => l.Trim().ToLower() != emailDesbanir.Trim().ToLower()).ToArray();
                System.IO.File.WriteAllLines(CaminhoListaNegra, novaLista);
            }
            return RedirectToAction("Index");
        }

        // Rota que exibe o ecrã para pedir a senha
        public IActionResult Portao()
        {
            // Verifica se o sistema está em período de bloqueio
            var bloqueioTicks = HttpContext.Session.GetString("BloqueioTronoAte");
            if (!string.IsNullOrEmpty(bloqueioTicks) && long.TryParse(bloqueioTicks, out long ticks))
            {
                var bloqueioAte = new DateTime(ticks);
                if (DateTime.Now < bloqueioAte)
                {
                    ViewBag.Erro = $"SISTEMA BLOQUEADO. Acesso trancado até {bloqueioAte:HH:mm:ss}.";
                    ViewBag.Bloqueado = true;
                    return View();
                }
                else
                {
                    // O tempo de bloqueio já passou, limpa o castigo
                    HttpContext.Session.Remove("BloqueioTronoAte");
                    HttpContext.Session.SetString("TentativasTrono", "0");
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult ValidarSenha(string senha)
        {
            // Lê a senha segura do appsettings.json
            string senhaSegura = _config["SenhaDoRei"] ?? "SenhaDeSeguranca123!";

            // Verifica bloqueio 
            var bloqueioTicks = HttpContext.Session.GetString("BloqueioTronoAte");
            if (!string.IsNullOrEmpty(bloqueioTicks) && long.TryParse(bloqueioTicks, out long ticks))
                if (DateTime.Now < new DateTime(ticks)) return RedirectToAction("Portao");

            // Compara com a senha do arquivo
            if (senha == senhaSegura)
            {
                HttpContext.Session.SetString("TronoDesbloqueado", "sim");
                HttpContext.Session.Remove("TentativasTrono");
                return RedirectToAction("Index");
            }

            // Lógica de falha
            int tentativas = int.Parse(HttpContext.Session.GetString("TentativasTrono") ?? "0");
            tentativas++;

            if (tentativas >= 3)
            {
                // Bloqueia por 30 minutos
                HttpContext.Session.SetString("BloqueioTronoAte", DateTime.Now.AddMinutes(30).Ticks.ToString());
                ViewBag.Erro = "MÁXIMO DE TENTATIVAS (3/3) ATINGIDO. SISTEMA BLOQUEADO POR 30 MINUTOS.";
                ViewBag.Bloqueado = true;
            }
            else
            {
                HttpContext.Session.SetString("TentativasTrono", tentativas.ToString());
                ViewBag.Erro = $"SENHA INCORRETA. TENTATIVA {tentativas}/3 REGISTADA.";
            }

            return View("Portao");
        }

        // Botão para trancar a sala ao sair
        public IActionResult Trancar()
        {
            HttpContext.Session.Remove("TronoDesbloqueado");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> GetMetricasHardware()
        {
            if (HttpContext.Session.GetString("TronoDesbloqueado") != "sim") return Unauthorized();

            // 1. LEITURA DE CPU (Nativo para Linux: Arch/Debian)
// 1. LEITURA DE CPU E TEMPERATURA
            double usoCpu = 0;
            string cpuTemp = "N/A";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Lê Uso de CPU
                    (long idle, long total) LerProcStat()
                    {
                        var linhaCpu = System.IO.File.ReadLines("/proc/stat").First();
                        var partes = linhaCpu.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        long idleTicks = long.Parse(partes[4]) + long.Parse(partes[5]);
                        long totalTicks = 0;
                        for (int i = 1; i < partes.Length; i++) totalTicks += long.Parse(partes[i]);
                        return (idleTicks, totalTicks);
                    }

                    var inicio = LerProcStat();
                    await Task.Delay(200); 
                    var fim = LerProcStat();

                    long diffTotal = fim.total - inicio.total;
                    long diffIdle = fim.idle - inicio.idle;
                    usoCpu = diffTotal > 0 ? (1.0 - ((double)diffIdle / diffTotal)) * 100 : 0;

                    // LÊ TEMPERATURA (Maioria das distros Linux guarda aqui em miligraus)
                    if (System.IO.File.Exists("/sys/class/thermal/thermal_zone0/temp"))
                    {
                        string tempStr = System.IO.File.ReadAllText("/sys/class/thermal/thermal_zone0/temp").Trim();
                        if (double.TryParse(tempStr, out double tempMilli))
                        {
                            cpuTemp = Math.Round(tempMilli / 1000.0, 1) + " °C";
                        }
                    }
                }
            }
            catch { usoCpu = -1; /* Caso não consiga ler */ }

            // 2. OUTRAS MÉTRICAS (Uptime, RAM, Disco, GPU)
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            
            var memoryInfo = GC.GetGCMemoryInfo();
            double totalRamGB = memoryInfo.TotalAvailableMemoryBytes / 1024.0 / 1024.0 / 1024.0;
            long appRamUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

            double totalDiscoGB = 0, livreDiscoGB = 0;
            try {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
                if (drive != null) {
                    totalDiscoGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                    livreDiscoGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                }
            } catch { }

            double percentagemDisco = totalDiscoGB > 0 ? ((totalDiscoGB - livreDiscoGB) / totalDiscoGB) * 100 : 0;

            string gpuUso = "N/A", vramUsada = "N/A", vramTotal = "N/A";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var process = new Process() {
                        StartInfo = new ProcessStartInfo {
                            FileName = "nvidia-smi",
                            Arguments = "--query-gpu=utilization.gpu,memory.used,memory.total --format=csv,noheader,nounits",
                            RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    var partes = output.Split(',');
                    if (partes.Length >= 3) {
                        gpuUso = partes[0].Trim(); vramUsada = partes[1].Trim(); vramTotal = partes[2].Trim();
                    }
                }
            }
            catch { gpuUso = "Off/Sem Driver"; }

            return Json(new {
                os = RuntimeInformation.OSDescription,
                uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m",
                cpuCores = Environment.ProcessorCount,
                cpuUso = Math.Round(usoCpu, 1), // Envia o uso da CPU arredondado para 1 casa decimal
                cpuTemp = cpuTemp,
                ramTotal = Math.Round(totalRamGB, 2),
                ramApp = appRamUsageMB,
                discoLivre = Math.Round(livreDiscoGB, 2),
                discoTotal = Math.Round(totalDiscoGB, 2),
                percDisco = Math.Round(percentagemDisco),
                gpuUso = gpuUso,
                vramUsada = vramUsada,
                vramTotal = vramTotal
            });
        }
    }
}