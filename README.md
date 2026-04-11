<div align="center">

# 🏰 StangeCastle

**Ecossistema web privado e portfólio digital interativo**

![.NET](https://img.shields.io/badge/.NET-ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Language-239120?style=for-the-badge&logo=csharp&logoColor=white)
![SQL](https://img.shields.io/badge/Database-SQL_Relacional-003B57?style=for-the-badge&logo=postgresql&logoColor=white)
![Status](https://img.shields.io/badge/Status-Em_Desenvolvimento-yellow?style=for-the-badge)

</div>

---

## 📖 Sobre o Projeto

O **StangeCastle** é uma aplicação web desenvolvida integralmente no ecossistema **.NET (C#)**, concebida com dois propósitos centrais:

- **Portfólio Digital Interativo** — Demonstração de proficiência técnica em engenharia de software, segurança da informação e arquitetura de sistemas.
- **Ecossistema Privado Multifuncional** — Plataforma pessoal com armazenamento em nuvem particular (*StangeTreasury*), organização de campanhas de TTRPG (*StangeLibrary*) e um assistente de IA especializado (*O Conselheiro*).

---

## 🧩 Módulos

### 🌐 Portfólio Público
Área de acesso público exibindo projetos, certificados e conquistas, com links diretos para repositórios no GitHub e documentações externas.

### 🔐 Autenticação e Controle de Acesso (RBAC)
Sistema de login/logout com controle de acesso baseado em funções (**Role-Based Access Control**), distinguindo no mínimo dois perfis: **Administrador** e **Usuário Comum**.

### 📁 Drive Pessoal — *StangeTreasury*
Armazenamento em nuvem privado com suporte a upload, download e exclusão de arquivos, além de criação e organização hierárquica de pastas. Totalmente isolado e protegido por autenticação.

### 📚 Gestão de TTRPG — *StangeLibrary*
Organizador e demonstrador de textos e imagens voltado para campanhas de *Tabletop Role-Playing Games*. Conta com um editor de texto rico para criação e formatação de anotações, lore e registros de sessão, com suporte a upload de imagens e controle de visibilidade (público/privado) por documento.

### 🤖 Hub de IA Local — *O Conselheiro* 
Interface de chat integrada a um modelo de linguagem hospedado localmente, especializado exclusivamente em dois domínios de conhecimento: **programação** e o **universo TTRPG da campanha em questão**. Conta com uma camada de segurança semântica via **SLM + RAG**, que contextualiza as respostas com base nesses domínios e atua como firewall contra *prompt injection*, bloqueando ativamente perguntas fora de escopo e requisições maliciosas.

---

## 👥 Perfis de Usuário

| Perfil | Acesso |
|---|---|
| **Público / Recrutadores** | Portfólio e documentos públicos da StangeLibrary |
| **Administrador** | Acesso irrestrito a todos os módulos e configurações |
| **Usuário Autenticado (Convidado/Jogador)** | Visualização de mídias privadas e uso limitado d'O Conselheiro |

---

## ⚙️ Requisitos Funcionais

| ID | Módulo | Descrição |
|---|---|---|
| RF01 | Portfólio | Exibição pública de projetos, certificados e conquistas |
| RF02 | Portfólio | Redirecionamentos para GitHub e documentações externas |
| RF03 | Autenticação | Login e logout de usuários registrados |
| RF04 | Autenticação | Controle de acesso baseado em funções (RBAC) |
| RF05 | Drive | Upload, download e exclusão de arquivos pelo Administrador |
| RF06 | Drive | Criação, renomeação e exclusão de pastas |
| RF07 | Drive | Bloqueio total para usuários não autenticados |
| RF08 | StangeLibrary | Editor de texto rico com suporte a imagens |
| RF09 | StangeLibrary | Controle de visibilidade por documento (público/privado) |
| RF10 | StangeLibrary | Criação, edição e exclusão restritas ao Administrador |
| RF11 | O Conselheiro | Interface de chat especializada em programação e no TTRPG da campanha |
| RF12 | O Conselheiro | Acesso restrito a usuários autenticados |

---

## 🛡️ Requisitos Não Funcionais

### Tecnologia e Arquitetura

| ID | Descrição |
|---|---|
| RNF01 | Frontend e Backend inteiramente em **C#** com **ASP.NET Core** (MVC, Razor Pages ou Blazor) |
| RNF02 | Banco de dados **SQL relacional** (SQL Server, PostgreSQL ou MySQL) para credenciais, metadados, conteúdo TTRPG e logs |

### Segurança da Informação

| ID | Descrição |
|---|---|
| RNF03 | Senhas armazenadas com **hashing + salt** (BCrypt ou `PasswordHasher` do ASP.NET Core Identity) |
| RNF04 | Defesas contra **OWASP Top 10**: tokens Anti-CSRF, consultas parametrizadas (SQL Injection) e sanitização de inputs (XSS) |
| RNF05 | Requisições a **O Conselheiro** filtradas por **Agente SLM intermediário com RAG**, atuando como firewall semântico contra *Prompt Injection* e perguntas fora do escopo definido (programação e TTRPG) |
| RNF06 | Arquivos do Drive **fora do `wwwroot`**, acessíveis apenas via validação de token/sessão na API |

### Desempenho e Disponibilidade

| ID | Descrição |
|---|---|
| RNF07 | **Rate Limiting** na API d'**O Conselheiro** por usuário (ex: máx. 5 mensagens/minuto) para evitar exaustão de CPU/GPU |
| RNF08 | Upload e download de arquivos via **Streams** para evitar `OutOfMemoryException` com arquivos grandes |

---

## 🏗️ Stack Tecnológica

```
Backend        → C# / ASP.NET Core
Frontend       → Razor Pages / Blazor / MVC (a definir)
Banco de Dados → SQL Relacional (SQL Server / PostgreSQL / MySQL)
Segurança      → ASP.NET Core Identity, Anti-CSRF, OWASP Top 10
IA             → O Conselheiro (LLM Local + SLM intermediário + RAG)
                 Domínios: Programação & TTRPG da Campanha
```

---

## 🗺️ Roadmap

- [x] Estrutura base da aplicação (ASP.NET Core)
- [x] Portfólio Público
- [ ] Módulo de Autenticação com RBAC
- [ ] StangeTreasury — Drive Pessoal
- [ ] StangeLibrary — CMS de TTRPG com editor rico
- [ ] O Conselheiro — Integração com LLM Local
- [ ] Camada de segurança SLM/RAG para O Conselheiro
- [ ] Rate Limiting e proteções de performance

---

## 📄 Licença

Este projeto é de uso pessoal e privado. Todos os direitos reservados ao autor.

---

<div align="center">
  <sub>Construído com C# e .NET • StangeCastle</sub>
</div>
