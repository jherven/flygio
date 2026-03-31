FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM node:22-alpine AS tailwind
WORKDIR /src
COPY src/Flygio/package.json src/Flygio/package-lock.json* ./
RUN npm ci --include=dev
COPY src/Flygio/Styles/ Styles/
COPY src/Flygio/Components/ Components/
COPY src/Flygio/tailwind.config.js ./
RUN npx @tailwindcss/cli -i Styles/app.css -o wwwroot/app.css --minify

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY src/Flygio/Flygio.csproj Flygio/
RUN dotnet restore Flygio/Flygio.csproj
COPY src/Flygio/ Flygio/
COPY --from=tailwind /src/wwwroot/app.css Flygio/wwwroot/app.css
WORKDIR /src/Flygio
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Flygio.dll"]
