# Mini Search Engine ECommerce (.NET + PostgreSQL + Elasticsearch)

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

---

# ECommerce Product Search with PostgreSQL + Elasticsearch + Background Job

Project ini menggunakan **PostgreSQL** sebagai sumber data utama (**source of truth**) dan **Elasticsearch** sebagai mesin pencarian.  
Sinkronisasi data dari PostgreSQL ke Elasticsearch dilakukan **setelah operasi CRUD berhasil disimpan ke database**, lalu diproses secara **background job** menggunakan **in-memory queue berbasis `Channel<T>`**.

---

## Daftar Isi

- [Arsitektur Singkat](#arsitektur-singkat)
- [Tujuan Desain](#tujuan-desain)
- [Kapan Indexing Berjalan](#kapan-indexing-berjalan)
- [Alur CRUD dan Sinkronisasi Elasticsearch](#alur-crud-dan-sinkronisasi-elasticsearch)
- [Jenis Antrian yang Digunakan](#jenis-antrian-yang-digunakan)
- [Flow Detail per Operasi](#flow-detail-per-operasi)
- [Struktur Project](#struktur-project)
- [Komponen Utama](#komponen-utama)
- [Urutan Eksekusi Saat Create / Update / Delete](#urutan-eksekusi-saat-create--update--delete)
- [Kenapa Tidak Reindex Ulang Semua Data](#kenapa-tidak-reindex-ulang-semua-data)
- [Kelebihan Pendekatan Ini](#kelebihan-pendekatan-ini)
- [Kekurangan Pendekatan Saat Ini](#kekurangan-pendekatan-saat-ini)
- [Saran Untuk Production Lebih Lanjut](#saran-untuk-production-lebih-lanjut)
- [Contoh Endpoint](#contoh-endpoint)
- [Catatan Penting](#catatan-penting)

---

## Arsitektur Singkat

Sistem ini menerapkan pola **Clean Architecture** secara sederhana:

- **Catalog.Api**
  - Endpoint / API layer
- **Catalog.Application**
  - Contract, interface, dan application service
- **Catalog.Domain**
  - Entity domain
- **Catalog.Infrastructure**
  - EF Core, PostgreSQL, Elasticsearch, background queue, background worker

PostgreSQL menyimpan data utama produk.  
Elasticsearch hanya dipakai untuk kebutuhan pencarian.

---

## Tujuan Desain

Tujuan utama implementasi ini adalah:

1. Menyimpan data produk ke PostgreSQL sebagai data utama.
2. Mengirim perubahan data ke Elasticsearch **tanpa reindex seluruh data**.
3. Memproses sinkronisasi Elasticsearch secara asynchronous melalui **background job**.
4. Menjaga performa saat data berjumlah sangat besar, misalnya jutaan record.

---

## Kapan Indexing Berjalan

### Indexing **tidak** berjalan saat aplikasi startup untuk semua data
Aplikasi **tidak melakukan reindex ulang seluruh data setiap `dotnet run`**.

### Indexing berjalan **setelah operasi CRUD berhasil disimpan ke PostgreSQL**
Artinya:

- **Create product**  
  Setelah data berhasil `INSERT` ke PostgreSQL, sistem membuat job background untuk melakukan **upsert** dokumen ke Elasticsearch.

- **Update product**  
  Setelah data berhasil `UPDATE` di PostgreSQL, sistem membuat job background untuk melakukan **upsert** ulang dokumen di Elasticsearch.

- **Delete product**  
  Setelah data berhasil `DELETE` di PostgreSQL, sistem membuat job background untuk melakukan **delete** dokumen di Elasticsearch.

### Kesimpulan singkat
Indexing Elasticsearch dilakukan **setelah transaksi perubahan data di PostgreSQL berhasil**, bukan sebelum, dan bukan dengan cara reindex seluruh index.

---

## Alur CRUD dan Sinkronisasi Elasticsearch

Aplikasi memakai pola berikut:

1. Request masuk ke endpoint API.
2. Endpoint memanggil `ProductCommandService`.
3. `ProductCommandService` melakukan perubahan data ke PostgreSQL melalui `AppDbContext`.
4. Setelah `SaveChangesAsync()` berhasil, service **enqueue** message ke antrian background.
5. Background worker membaca message dari antrian.
6. Worker memanggil service sinkronisasi Elasticsearch.
7. Elasticsearch diperbarui hanya untuk item yang berubah.

---

## Jenis Antrian yang Digunakan

Saat ini antrian yang digunakan adalah:

**In-memory queue** menggunakan `System.Threading.Channels.Channel<T>`

### Implementasi
Queue diimplementasikan pada class:

- `InMemoryProductIndexQueue`

dan message yang dikirim ke queue adalah:

- `ProductIndexMessage`

### Isi message queue
Message berisi informasi seperti:

- `ProductId`
- `Action`

Contoh action:

- `Upsert`
- `Delete`

### Kenapa memakai `Channel<T>`
`Channel<T>` dipilih karena:

- ringan
- bawaan .NET
- tidak perlu dependency tambahan
- cocok untuk background producer-consumer
- mudah dipakai untuk proof of concept dan aplikasi awal

### Catatan penting
Karena queue ini **in-memory**, maka:

- job ada di memori aplikasi
- jika aplikasi crash / restart sebelum job diproses, job bisa hilang
- ini cocok untuk tahap awal atau development
- untuk production yang lebih kuat, sebaiknya ditingkatkan ke **Outbox Pattern** atau message broker

---

## Flow Detail per Operasi

## 1. Create Product

### Kapan indexing dilakukan
Setelah data berhasil masuk ke PostgreSQL.

### Urutan proses
1. API menerima request create product.
2. `ProductCommandService.CreateAsync()` membuat entity `Product`.
3. Product disimpan ke PostgreSQL.
4. `SaveChangesAsync()` berhasil.
5. Service mengirim message `Upsert` ke queue.
6. Background worker membaca message.
7. Worker mengambil data product dari PostgreSQL.
8. Worker melakukan `PUT /{index}/_doc/{id}` ke Elasticsearch.

### Hasil
- Data baru ada di PostgreSQL.
- Dokumen baru dibuat di Elasticsearch.
- Tidak ada reindex semua data.

---

## 2. Update Product

### Kapan indexing dilakukan
Setelah data berhasil diupdate di PostgreSQL.

### Urutan proses
1. API menerima request update product.
2. `ProductCommandService.UpdateAsync()` mencari data berdasarkan `Id`.
3. Field product diperbarui.
4. `SaveChangesAsync()` berhasil.
5. Service mengirim message `Upsert` ke queue.
6. Background worker membaca message.
7. Worker mengambil versi terbaru product dari PostgreSQL.
8. Worker melakukan `PUT /{index}/_doc/{id}` ke Elasticsearch.

### Hasil
- Data di PostgreSQL berubah.
- Dokumen di Elasticsearch ikut diperbarui.
- Tidak perlu delete lalu insert manual; cukup **upsert**.

---

## 3. Delete Product

### Kapan indexing dilakukan
Setelah data berhasil dihapus dari PostgreSQL.

### Urutan proses
1. API menerima request delete product.
2. `ProductCommandService.DeleteAsync()` mencari product.
3. Product dihapus dari PostgreSQL.
4. `SaveChangesAsync()` berhasil.
5. Service mengirim message `Delete` ke queue.
6. Background worker membaca message.
7. Worker memanggil Elasticsearch untuk menghapus dokumen berdasarkan `Id`.

### Hasil
- Data di PostgreSQL hilang.
- Dokumen di Elasticsearch ikut dihapus.
- Tidak ada proses reindex massal.

---

## Struktur Project

Contoh struktur utama:

```text
Catalog.Api
 ├─ Endpoints
 │   └─ ProductEndpoints.cs

Catalog.Application
 ├─ Contracts
 │   ├─ CreateProductRequest.cs
 │   ├─ UpdateProductRequest.cs
 │   └─ ProductIndexMessage.cs
 ├─ Interfaces
 │   ├─ IProductCommandService.cs
 │   ├─ IProductIndexQueue.cs
 │   └─ IElasticProductSyncService.cs
 └─ Services
     └─ ProductCommandService.cs

Catalog.Domain
 └─ Entities
     └─ Product.cs

Catalog.Infrastructure
 ├─ Persistence
 │   └─ AppDbContext.cs
 ├─ Search
 │   └─ ElasticProductSyncService.cs
 ├─ BackgroundJobs
 │   ├─ InMemoryProductIndexQueue.cs
 │   └─ ProductIndexBackgroundService.cs
 └─ DependencyInjection
     └─ InfrastructureServiceRegistration.cs