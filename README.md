# 📚 SchulBib - Datenschutzkonforme Bibliotheksverwaltung für Schulen

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/maui)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)

> **Moderne, datenschutzkonforme und 100% kostenlose Bibliotheksverwaltung für Schulen.**  
> Entwickelt für deutsche Schulen mit vollständiger Offline-Funktionalität.

## 🎯 Projektübersicht

SchulBib ist eine **Open Source Bibliotheksverwaltung**, die speziell für deutsche Schulen entwickelt wurde. Mit Fokus auf **Datenschutz**, **Offline-Betrieb** und **Benutzerfreundlichkeit** bietet es eine moderne Alternative zu teuren, cloud-basierten Lösungen.

### ✨ Highlights

- 🔒 **Datenschutz by Design** - Alle Daten bleiben in der Schule
- 🌐 **Offline-First** - Funktioniert auch ohne Internetverbindung
- 💰 **100% kostenlos** - Keine Lizenzgebühren, keine versteckten Kosten
- 📱 **Cross-Platform** - iOS, Android, Windows mit .NET MAUI
- 🐳 **Docker-Ready** - Einfache Bereitstellung mit Containern

## 🚀 Features

### 👨‍🎓 Für Schüler
- **QR-Code-Login** - Einfacher Zugang per Scan oder ID
- **Intuitive Buchausleihe** - Kamera-basierter Scanner
- **Meine Bücher** - Übersicht mit Cover-Bildern und Rückgabedaten
- **Smart Reminders** - Automatische Erinnerungen vor Rückgabetermin
- **Offline-Modus** - Auch bei WLAN-Problemen nutzbar

### 👩‍🏫 Für Lehrer & Bibliothekare
- **Smart Book Registration** - ISBN scannen → Automatische Buchinfos
- **Flexible Server-Optionen** - Lokal, Cloud oder Hybrid
- **Umfassende Verwaltung** - Schüler, Bücher, Ausleihen
- **Intelligente Berichte** - Statistiken, überfällige Bücher, Nutzungsanalysen
- **Bulk-Operationen** - CSV-Import, Batch-QR-Generierung

### 🔧 Für IT-Administratoren
- **Docker-Container** - Ein-Klick-Installation mit Docker Compose
- **Flexibles Hosting** - Lokal auf Schul-PC oder Cloud-Deployment
- **Automatische Backups** - Verschlüsselte Datensicherung
- **Health-Monitoring** - System-Status und Performance-Überwachung

## 🛠️ Technologie-Stack

```
Frontend:     .NET MAUI 8.0 (iOS, Android, Windows)
Backend:      ASP.NET Core 8.0 Web API
Datenbank:    SQLite mit AES-256 Verschlüsselung
QR-Codes:     ZXing.Net.Maui
ISBN-Lookup:  Open Library API (kostenlos)
Container:    Docker & Docker Compose
```

## 📦 Installation

### Mit Docker (Empfohlen)

```bash
# Repository klonen
git clone https://github.com/Agredo/SchulBib.git
cd SchulBib

# Mit Docker Compose starten
docker-compose up -d

# Oder einzelner Container
docker run -d -p 5000:80 schulbib/api:latest
```

### Lokaler Betrieb

```bash
# Manuell ausführen
dotnet run --project src/SchulBib.Api

# Als Windows Service
sc create SchulBib binPath="C:\SchulBib\SchulBib.Api.exe"
sc start SchulBib
```

### Cloud-Deployment

```bash
# Azure App Service
az webapp up --name schulbib --resource-group SchulBib

# AWS mit Docker
aws ecs create-service --service-name schulbib --task-definition schulbib:1
```

## 🐳 Docker-Konfiguration

### docker-compose.yml
```yaml
version: '3.8'
services:
  schulbib-api:
    image: schulbib/api:latest
    ports:
      - "5000:80"
    volumes:
      - ./data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Data Source=/app/data/schulbib.db
```

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SchulBib.Api/SchulBib.Api.csproj", "SchulBib.Api/"]
RUN dotnet restore "SchulBib.Api/SchulBib.Api.csproj"

COPY . .
WORKDIR "/src/SchulBib.Api"
RUN dotnet build "SchulBib.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchulBib.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchulBib.Api.dll"]
```

## 🏗️ Entwicklung

### Entwicklungsumgebung einrichten

```bash
# Voraussetzungen
- .NET 8.0 SDK
- Visual Studio 2022 oder VS Code
- .NET MAUI Workload
- Docker Desktop

# Projekt klonen und Abhängigkeiten installieren
git clone https://github.com/Agredo/SchulBib.git
cd SchulBib
dotnet workload install maui
dotnet restore

# Backend starten
cd src/SchulBib.Api
dotnet run

# Mobile Apps (iOS/Android)
cd src/SchulBib.Mobile
dotnet build -f net8.0-ios    # iOS
dotnet build -f net8.0-android # Android

# Docker Development
docker-compose -f docker-compose.dev.yml up -d
```

### Projektstruktur
```
SchulBib/
├── 📱 src/
│   ├── SchulBib.Mobile/       # .NET MAUI Apps (Schüler & Lehrer)
│   ├── SchulBib.Api/          # ASP.NET Core Backend
│   ├── SchulBib.Core/         # Shared Business Logic
│   └── SchulBib.Infrastructure/ # Database & External Services
├── 🐳 docker/                 # Container-Konfiguration
├── 📚 docs/                   # Dokumentation
├── 🧪 tests/                  # Unit & Integration Tests
├── 🔧 tools/                  # Scripts & Utilities
├── docker-compose.yml         # Production Setup
└── docker-compose.dev.yml     # Development Setup
```

## 🤝 Beitragen

Wir freuen uns über jeden Beitrag! SchulBib ist ein Community-Projekt.

### Wie Sie beitragen können:
- 🐛 **Bug-Reports:** [Issue erstellen](https://github.com/Agredo/SchulBib/issues)
- 💡 **Feature-Requests:** [Discussion starten](https://github.com/Agredo/SchulBib/discussions)
- 💻 **Code-Beiträge:** [Pull Request einreichen](CONTRIBUTING.md)
- 📖 **Dokumentation:** Wiki-Artikel schreiben
- 🌍 **Übersetzungen:** Andere Sprachen hinzufügen

### Entwickler-Guidelines
Bitte lesen Sie unsere [🤝 Beitrag-Richtlinien](CONTRIBUTING.md) vor dem ersten Pull Request.

## 📄 Lizenz

```
MIT License

Copyright (c) 2025 SchulBib Community

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

[...]
```

**Vollständige Lizenz:** [LICENSE](LICENSE)

---
