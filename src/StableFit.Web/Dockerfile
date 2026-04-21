FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/StableFit.Web/StableFit.Web.csproj", "src/StableFit.Web/"]
RUN dotnet restore "src/StableFit.Web/StableFit.Web.csproj"
COPY . .
WORKDIR "/src/src/StableFit.Web"
RUN dotnet build "StableFit.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StableFit.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY src/StableFit.Web/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
