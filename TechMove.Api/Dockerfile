FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TechMove.Api/TechMove.Api.csproj TechMove.Api/
RUN dotnet restore TechMove.Api/TechMove.Api.csproj

COPY . .
WORKDIR /src/TechMove.Api
RUN dotnet publish TechMove.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TechMove.Api.dll"]
