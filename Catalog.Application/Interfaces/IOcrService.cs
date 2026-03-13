namespace Catalog.Application.Interfaces;

// <summary>
// service untuk melakukan OCR menggunakan Tesseract, yang akan mengekstrak teks dari gambar yang diunggah oleh pengguna.
// Ini akan memungkinkan pengguna untuk mencari produk berdasarkan teks yang terdapat dalam gambar, seperti nama produk atau merek.
// </summary>
public interface IOcrService
{
    Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default);
}