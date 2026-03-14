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

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /app/runtimes/linux-x64/native \
    && cp /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/libleptonica-1.82.0.so \
    && cp /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/runtimes/linux-x64/native/libleptonica-1.82.0.so \
    && cp /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/libtesseract.so \
    && cp /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/runtimes/linux-x64/native/libtesseract.so

ENTRYPOINT ["sh", "-c", "export LD_LIBRARY_PATH=/app:/app/runtimes/linux-x64/native:/usr/lib/x86_64-linux-gnu && dotnet Catalog.Api.dll"]