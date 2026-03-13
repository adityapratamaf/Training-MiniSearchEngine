using Catalog.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Tesseract;

namespace Catalog.Infrastructure.Ocr;

// <summary>
// service untuk melakukan OCR menggunakan Tesseract, yang akan mengekstrak teks dari gambar yang diunggah oleh pengguna.
// Ini akan memungkinkan pengguna untuk mencari produk berdasarkan teks yang terdapat dalam gambar, seperti nama produk atau merek.
// </summary>
public class TesseractOcrService : IOcrService
{
    private readonly string _tessDataPath;

    // Konstruktor untuk TesseractOcrService yang menerima IHostEnvironment untuk menentukan lokasi folder tessdata.
    public TesseractOcrService(IHostEnvironment env)
    {
        _tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
    }

    // Metode untuk mengekstrak teks dari gambar menggunakan Tesseract OCR. Ini memuat gambar dari byte array, memprosesnya dengan Tesseract, dan mengembalikan teks yang diekstrak.
    public Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"TessdataPath: {_tessDataPath}");

        // Validasi untuk memastikan folder tessdata dan file eng.traineddata ada sebelum memproses gambar.
        if (!Directory.Exists(_tessDataPath))
            throw new DirectoryNotFoundException($"Tessdata folder not found: {_tessDataPath}");

        // Pastikan file eng.traineddata ada dan tidak kosong, karena ini diperlukan untuk Tesseract agar dapat mengenali teks dalam gambar.
        var engPath = Path.Combine(_tessDataPath, "eng.traineddata");
        if (!File.Exists(engPath))
            throw new FileNotFoundException($"eng.traineddata not found: {engPath}");

        // Periksa ukuran file eng.traineddata untuk memastikan bahwa file tersebut tidak kosong, karena file yang kosong akan menyebabkan Tesseract gagal dalam proses OCR.
        var fileInfo = new FileInfo(engPath);
        Console.WriteLine($"eng size: {fileInfo.Length} bytes");

        // Jika file eng.traineddata kosong, lemparkan pengecualian untuk mencegah Tesseract mencoba memproses gambar dengan data pelatihan yang tidak valid, yang akan menghasilkan kesalahan.
        if (fileInfo.Length == 0)
            throw new InvalidOperationException($"eng.traineddata is empty: {engPath}");

        // SetEnvironmentVariable digunakan untuk memastikan bahwa Tesseract dapat menemukan data pelatihan yang diperlukan untuk melakukan OCR. Ini penting karena Tesseract membutuhkan akses ke file-file ini untuk mengenali teks dalam gambar.
        Environment.SetEnvironmentVariable("TESSDATA_PREFIX", _tessDataPath);

        // Proses OCR menggunakan Tesseract. Ini memuat gambar dari byte array, memprosesnya dengan TesseractEngine, dan mengembalikan teks yang diekstrak. Pastikan untuk menangani sumber daya dengan benar menggunakan 'using' untuk memastikan bahwa semua sumber daya dibersihkan setelah digunakan.
        using var image = Pix.LoadFromMemory(fileBytes);
        using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.LstmOnly);
        using var page = engine.Process(image);

        // Ambil teks yang diekstrak dari gambar dan kembalikan sebagai hasil. Pastikan untuk menangani kasus di mana teks mungkin null dengan memberikan nilai default string.Empty, dan trim hasilnya untuk menghapus spasi yang tidak perlu.
        var text = page.GetText() ?? string.Empty;
        return Task.FromResult(text.Trim());
    }
}