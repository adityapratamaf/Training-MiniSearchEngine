# Mini Search Engine Catalog (.NET + PostgreSQL + Elasticsearch)

Mini project ini adalah **search engine sederhana untuk katalog produk** menggunakan:

* **.NET 9 / ASP.NET Core Web API**
* **Clean Architecture**
* **PostgreSQL** sebagai database utama
* **Elasticsearch** sebagai search engine
* **Kibana** untuk monitoring dan debugging Elasticsearch
* **Swagger** untuk testing API
* **Seeder + Factory** untuk membuat **1000 dummy product**

Project ini mendemonstrasikan implementasi fitur:

* search product
* filter category
* filter price range
* sort price
* pagination

---

# Architecture Overview

Arsitektur sistem:

```
Client / Swagger
       │
       ▼
ASP.NET Core API (.NET 9)
       │
       ├── PostgreSQL (source of truth)
       │
       └── Elasticsearch (search engine)
                │
                ▼
              Kibana
```

Flow data:

```
Seeder → PostgreSQL
           │
           ▼
     Bulk Indexing
           │
           ▼
      Elasticsearch
           │
           ▼
         Search API
```

---

# Tech Stack

| Technology         | Description             |
| ------------------ | ----------------------- |
| .NET 9             | Backend API             |
| ASP.NET Core       | REST API                |
| PostgreSQL         | Primary database        |
| Elasticsearch      | Search engine           |
| Kibana             | Elasticsearch dashboard |
| Swagger            | API testing             |
| Clean Architecture | Layered architecture    |

---

# Project Structure

```
MiniSearchEngine
│
├── Catalog.Api
│   ├── Controllers
│   ├── Program.cs
│   └── appsettings.json
│
├── Catalog.Application
│   ├── Contracts
│   ├── Dtos
│   └── Interfaces
│
├── Catalog.Domain
│   └── Entities
│
├── Catalog.Infrastructure
│   ├── Persistence
│   ├── Seeding
│   ├── Search
│   └── DependencyInjection
│
└── docker-compose.yml
```

---

# Requirements

Pastikan environment sudah memiliki:

* .NET SDK 9+
* PostgreSQL
* Docker
* Elasticsearch
* Kibana

---

# Setup PostgreSQL

Buat database baru:

```
mini_search_db
```

Contoh connection string di `appsettings.json`:

```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Database=mini_search_db;Username=postgres;Password=postgres"
}
```

Sesuaikan username/password dengan PostgreSQL lokal.

---

# Setup Elasticsearch

Jika menggunakan Docker lokal:

## docker-compose.yml

```
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:9.0.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
```

Jalankan:

```
docker compose up -d
```

Cek Elasticsearch:

```
http://localhost:9200
```

---

# Jika menggunakan Elasticsearch Server

Misalnya:

```
https://elastic.minimdev.com
```

Update `appsettings.json`:

```json
"Elastic": {
  "BaseUrl": "https://elastic.minimdev.com",
  "IndexName": "products"
}
```

---

# Setup Kibana

Buka di browser:

```
https://kibana.minimdev.com
```

Menu yang sering digunakan:

* Dev Tools
* Discover
* Index Management

---

# Install Dependencies

Jalankan restore:

```
dotnet restore
```

Build project:

```
dotnet build
```

---

# Run Migration

Buat migration:

```
dotnet ef migrations add InitialCreate \
--project Catalog.Infrastructure \
--startup-project Catalog.Api \
--output-dir Persistence/Migrations
```

Update database:

```
dotnet ef database update \
--project Catalog.Infrastructure \
--startup-project Catalog.Api
```

---

# Run Application

Jalankan API:

```
dotnet run --project Catalog.Api
```

Swagger akan tersedia di:

```
http://localhost:5213/swagger
```

---

# Seeder Data

Saat aplikasi dijalankan:

1. Migration dijalankan
2. Seeder akan membuat **1000 dummy products**
3. Data disimpan di PostgreSQL
4. Data di **bulk index ke Elasticsearch**

Seeder menggunakan:

* Factory Pattern
* Random data generator

---

# Elasticsearch Index

Index yang dibuat:

```
products
```

Mapping contoh:

```
id            keyword
name          text
category      keyword
price         double
description   text
createdAtUtc  date
```

---

# Search API

Endpoint:

```
GET /api/products/search
```

---

# Search Product

Contoh:

```
/api/products/search?q=laptop
```

---

# Filter Category

```
/api/products/search?category=electronics
```

---

# Filter Price Range

```
/api/products/search?minPrice=1000000&maxPrice=5000000
```

---

# Sorting

```
/api/products/search?sortPrice=asc
```

atau

```
/api/products/search?sortPrice=desc
```

---

# Pagination

```
/api/products/search?page=1&pageSize=10
```

---

# Full Query Example

```
/api/products/search?q=laptop&category=electronics&minPrice=1000000&maxPrice=5000000&sortPrice=asc&page=1&pageSize=10
```

---

# Elasticsearch Query Example

Query yang dihasilkan API kira-kira seperti ini:

```
GET products/_search
{
  "from": 0,
  "size": 10,
  "sort": [
    { "price": "asc" }
  ],
  "query": {
    "bool": {
      "must": [
        {
          "multi_match": {
            "query": "laptop",
            "fields": ["name","description"]
          }
        }
      ],
      "filter": [
        {
          "term": {
            "category": "electronics"
          }
        }
      ]
    }
  }
}
```

---

# Testing via Kibana

Masuk ke **Dev Tools**

### Check Index

```
GET _cat/indices?v
```

---

### Check Data Count

```
GET products/_count
```

---

### View Data

```
GET products/_search
{
 "size": 5
}
```

---

### Search Product

```
GET products/_search
{
  "query": {
    "match": {
      "name": "laptop"
    }
  }
}
```

---

# Reset Index

Jika ingin reset data:

```
DELETE products
```

Lalu jalankan kembali API untuk **reindex data**.

---

# Development Workflow

Biasanya workflow developer:

```
1. Test query di Kibana
2. Implement query di .NET
3. Test di Swagger
```

---

# Best Practices

Gunakan:

* PostgreSQL sebagai **source of truth**
* Elasticsearch sebagai **search engine**
* Reindex data ketika schema berubah

---

# Future Improvements

Beberapa fitur yang bisa ditambahkan:

### Autocomplete Search

```
search-as-you-type
```

---

### Relevance Ranking

```
boost
function score
```

---

### Background Sync

Sinkronisasi PostgreSQL → Elasticsearch menggunakan:

* Kafka
* RabbitMQ
* Background Worker

---

### Kibana Dashboard

Visualisasi data seperti:

* jumlah produk per kategori
* price distribution
* search analytics

---

# Conclusion

Project ini mendemonstrasikan implementasi **search engine menggunakan Elasticsearch** dalam aplikasi **ASP.NET Core dengan Clean Architecture**.

Fitur utama yang diimplementasikan:

* full-text search
* filtering
* sorting
* pagination
* bulk indexing
