# Le Grand Cinema - Angular 20 + ASP.NET Core 10, single container
FROM node:22-alpine AS frontend-build
WORKDIR /src/frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build -- --configuration production

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY backend/Cinema.Api/Cinema.Api.csproj backend/Cinema.Api/
RUN dotnet restore backend/Cinema.Api/Cinema.Api.csproj
COPY backend/Cinema.Api/ backend/Cinema.Api/
RUN dotnet publish backend/Cinema.Api/Cinema.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/frontend/dist/frontend/browser/ ./wwwroot/
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cinema.Api.dll"]
