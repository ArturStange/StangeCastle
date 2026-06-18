# Usa a imagem oficial do SDK para compilar o código
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Usa a imagem oficial do Runtime para rodar o site (mais leve)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define as variáveis de ambiente para a porta padrão 8080 do .NET 8
ENV ASPNETCORE_HTTP_PORTS=4233
EXPOSE 4233

ENTRYPOINT ["dotnet", "StangeCastle.dll"] 
# AVISO: Mude "SeuProjeto.dll" para o nome exato do seu arquivo compilado (normalmente o nome da pasta do projeto)