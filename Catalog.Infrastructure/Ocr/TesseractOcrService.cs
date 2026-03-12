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

        if (!Directory.Exists(_tessDataPath))
            throw new DirectoryNotFoundException($"Tessdata folder not found: {_tessDataPath}");

        var engPath = Path.Combine(_tessDataPath, "eng.traineddata");
        if (!File.Exists(engPath))
            throw new FileNotFoundException($"eng.traineddata not found: {engPath}");

        var fileInfo = new FileInfo(engPath);
        Console.WriteLine($"eng size: {fileInfo.Length} bytes");

        if (fileInfo.Length == 0)
            throw new InvalidOperationException($"eng.traineddata is empty: {engPath}");

        Environment.SetEnvironmentVariable("TESSDATA_PREFIX", _tessDataPath);

        using var image = Pix.LoadFromMemory(fileBytes);
        using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.LstmOnly);
        using var page = engine.Process(image);

        var text = page.GetText() ?? string.Empty;
        return Task.FromResult(text.Trim());
    }
}