# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution dan file project
COPY ["MiniSearchEngine.slnx", "./"]
COPY ["Catalog.Api/Catalog.Api.csproj", "Catalog.Api/"]
COPY ["Catalog.Application/Catalog.Application.csproj", "Catalog.Application/"]
COPY ["Catalog.Domain/Catalog.Domain.csproj", "Catalog.Domain/"]
COPY ["Catalog.Infrastructure/Catalog.Infrastructure.csproj", "Catalog.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Catalog.Api/Catalog.Api.csproj"

# Copy semua source
COPY . .

# Build
WORKDIR /src/Catalog.Api
RUN dotnet publish "Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng \
    tesseract-ocr-ind \
    libtesseract-dev \
    libleptonica-dev \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV LD_LIBRARY_PATH=/app/runtimes/linux-x64/native:/app:/usr/lib/x86_64-linux-gnu
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /app/runtimes/linux-x64/native \
    && ln -sf /usr/lib/x86_64-linux-gnu/libleptonica.so /app/runtimes/linux-x64/native/libleptonica-1.82.0.so \
    && ln -sf /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/runtimes/linux-x64/native/libtesseract.so

ENTRYPOINT ["dotnet", "Catalog.Api.dll"]