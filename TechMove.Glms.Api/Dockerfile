FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TechMove.Glms.Api/TechMove.Glms.Api.csproj", "TechMove.Glms.Api/"]
COPY ["TechMove.Glms.Core/TechMove.Glms.Core.csproj", "TechMove.Glms.Core/"]
RUN dotnet restore "TechMove.Glms.Api/TechMove.Glms.Api.csproj"
COPY . .
WORKDIR "/src/TechMove.Glms.Api"
RUN dotnet build "TechMove.Glms.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TechMove.Glms.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechMove.Glms.Api.dll"]
