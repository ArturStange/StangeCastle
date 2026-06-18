# Stange Castle - Homelab Portal & Portfolio

Um portal web unificado, leve e de alta performance construído em **ASP.NET Core MVC**. Este projeto serve como a espinha dorsal de um Homelab pessoal hospedado em Linux, combinando um portfólio profissional, uma Wiki de RPG (com CMS próprio) e um robusto Painel de Administração de Servidor com monitoramento de hardware em tempo real e gerenciador de arquivos seguro.

A interface foi projetada inteiramente com **HTML5 e CSS3 Puro**, focada no uso de variáveis CSS para criar uma estética *retro-terminal dark fantasy*, garantindo máxima leveza e dispensando o uso de frameworks frontend pesados.

## 🚀 Tecnologias Utilizadas

*   **Backend:** C#, .NET 8, ASP.NET Core MVC
*   **Frontend:** HTML5, CSS3 (Variáveis Globais, Flexbox/Grid, Animações Nativas), Vanilla JavaScript (Fetch API para assincronicidade)
*   **Autenticação:** Google OAuth 2.0 (Autenticação baseada em Cookies)
*   **Ambiente Alvo:** Linux (Debian / Arch) via Cloudflare Tunnels
*   **Monitoramento:** Interações nativas com o Kernel Linux (`/proc/stat`), `.NET System.Diagnostics`, e chamadas via CLI (`nvidia-smi`).

---

## ⚙️ Arquitetura e Funcionalidades Principais

### 1. Sistema de Autenticação e Whitelist de Segurança
A segurança do portal é baseada em *Role-Based Access Control* (RBAC) integrado ao **Google OAuth 2.0**.
*   **Whitelist Dinâmica:** O acesso de Administrador é concedido em tempo de execução via interceptação de `Claims`, cruzando o e-mail validado pelo Google com uma lista de donos no servidor.
*   **Flat-File Ban System:** Um sistema customizado e ágil de auditoria que registra usuários cadastrados e bloqueia tentativas de acesso (Lista Negra) consultando arquivos `.txt`, rejeitando conexões antes mesmo da criação do ticket de sessão.
*   **Rate Limiting & Lockout:** A "Sala do Trono" possui uma segunda camada de autenticação (Senha Mestra baseada em Sessão). Inclui proteção contra força bruta (Lockout automático de 30 minutos após 3 falhas).

### 2. Monitoramento de Hardware Nativos (Real-Time)
Desenvolvido para máxima eficiência sem pacotes de terceiros, consumindo JSONs gerados por um endpoint assíncrono e atualizados no frontend via JS:
*   **CPU Parsing (Linux Kernel):** Leitura de ciclos diretamente do `/proc/stat` do Linux com delta de tempo (200ms) para calcular a porcentagem de uso real sem sobrecarregar processos do sistema.
*   **Gestão de Memória:** Leitura profunda com `GC.GetGCMemoryInfo()` para capturar a RAM total da máquina e `WorkingSet64` para monitorar o uso de memória da própria aplicação web.
*   **Integração GPU NVIDIA:** Chamadas ocultas de processos ao `nvidia-smi` recuperando dados em CSV para exibir uso de processamento gráfico e VRAM em tempo real.
*   **Storage Metrics:** Interação com `DriveInfo` para mapear espaços do disco principal com barras dinâmicas de preenchimento.

### 3. Stange Treasure (Gestor de Arquivos Seguro)
Um *Drive* pessoal construído do zero, encapsulado de forma a impedir falhas clássicas de segurança web.
*   **Anti-Directory Traversal:** Validação estrita usando `Path.GetFullPath` comparada à raiz do cofre, impossibilitando que requisições maliciosas (ex: `../../etc/passwd`) leiam o OS.
*   **Isolamento Público:** Os arquivos são salvos fora do diretório `wwwroot`. Arquivos estáticos (como imagens e PDFs) são servidos dinamicamente e autorizados via `PhysicalFileResult`, protegidos por rotas baseadas na role `[Authorize(Roles = "Admin")]`.
*   **Funcionalidades:** Suporte a criação de pastas aninhadas, *upload* direcionado, realocação dinâmica de arquivos/pastas, visualização de imagens in-line e download de *streams* de dados.

### 4. Custom CMS & Wiki Single Page
*   **Editor de Textos Nativo:** O sistema permite editar arquivos de marcação (`.html`, `.txt`, `.md`) do servidor diretamente de um `textarea` no Dashboard Admin, sobrescrevendo a base da Wiki sem necessidade de acesso SSH ou FTP.
*   **UX Otimizada:** A tela do cliente ("Biblioteca") renderiza o conteúdo como uma *Single Page Application* nativa, usando `position: sticky`, ancoragem suave e scripts que colapsam totalmente a navegação para maximizar o espaço de leitura de grandes volumes de texto (*lore*).

---

## 🛠️ Como Executar Localmente

**Pré-requisitos:** .NET SDK instalado e Credenciais do Google Cloud Console.

    1.  Clone o repositório:
    ```bash
        git clone [https://github.com/ArturStange/Homelab-StangeCastle.git](https://github.com/ArturStange/Homelab-                      StangeCastle.git)
        cd Homelab-StangeCastle
        ```  
    2.  Configure o `appsettings.json` ou use os `.NET User Secrets` para injetar suas chaves OAuth:
    ```json
        {
          "Authentication": {
            "Google": {
              "ClientId": "SEU_CLIENT_ID",
              "ClientSecret": "SEU_CLIENT_SECRET"
            }
          },
          "Whitelist": {
            "Admins": ["seu-email@gmail.com"]
          }
        }
        ```
    3.  Inicie a aplicação:
    ```bash
        dotnet run
        ```
    4.  O sistema criará automaticamente as pastas e arquivos lógicos essenciais (`UsuariosCadastrados.txt`, `WikiContent.html`,     `StangeTreasure_Drive/`) durante o primeiro uso e chamadas às páginas.
    ---

## 🔮 Roadmap (O Futuro do Reino)
O projeto serve como plataforma base que será gradualmente expandida para:
*   Integração e monitoramento de *Server Status* do jogo Minecraft.
*   Lançamento do **O Conselheiro**, interface de Chat interligada a modelos de **LLMs (SLMs) locais** para pesquisa independente em IA.
*   Implantação de **Távola Redonda**, um VTT (*Virtual Tabletop*) nativo para rodar o sistema de RPG *Mythos* na web.

---

Desenvolvido por **Artur Stange Félix** — Engenheiro de Software, Pesquisador de IA (SLM/RAG) e Desenvolvedor de Jogos.
