FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/StableFit.API/StableFit.API.csproj", "src/StableFit.API/"]
COPY ["src/StableFit.Application/StableFit.Application.csproj", "src/StableFit.Application/"]
COPY ["src/StableFit.Domain/StableFit.Domain.csproj", "src/StableFit.Domain/"]
COPY ["src/StableFit.Infrastructure/StableFit.Infrastructure.csproj", "src/StableFit.Infrastructure/"]
RUN dotnet restore "src/StableFit.API/StableFit.API.csproj"
COPY . .
WORKDIR "/src/src/StableFit.API"
RUN dotnet build "StableFit.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StableFit.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StableFit.API.dll"]
