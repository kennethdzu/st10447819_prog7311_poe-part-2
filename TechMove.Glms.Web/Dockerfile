FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TechMove.Glms.Web/TechMove.Glms.Web.csproj", "TechMove.Glms.Web/"]
COPY ["TechMove.Glms.Core/TechMove.Glms.Core.csproj", "TechMove.Glms.Core/"]
RUN dotnet restore "TechMove.Glms.Web/TechMove.Glms.Web.csproj"
COPY . .
WORKDIR "/src/TechMove.Glms.Web"
RUN dotnet build "TechMove.Glms.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TechMove.Glms.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechMove.Glms.Web.dll"]
