using Catalog.Application.Contracts;
using Catalog.Application.Interfaces;
using System.Threading.Channels;

namespace Catalog.Infrastructure.BackgroundJobs;

// <summary>
// implementasi in-memory untuk antrian indexing produk menggunakan Channel<T> dari System.Threading.Channels,
// Dengan menggunakan Channel<ProductIndexMessage>, kita bisa membuat antrian yang khusus untuk pesan indexing produk,

// implementasi antrian indexing produk menggunakan Channel<ProductIndexMessage>.
// Pada generic Channel<T>, T di sini adalah ProductIndexMessage,
// sehingga channel ini hanya dapat menampung pesan indexing produk.
// Pendekatan ini cocok untuk pola producer-consumer, di mana:
// - producer menambahkan pesan ke antrian
// - background service membaca dan memproses pesan dari antrian

// Channel<ProductIndexMessage> sama saja dengan Channel<T> = “Oke, untuk channel ini, tipe datanya adalah ProductIndexMessage.”
// T = nama placeholder | ProductIndexMessage = tipe nyata yang mengisi placeholder itu

// Analogi: cetakan botol
// Bayangkan ada mesin pembuat botol dengan label:

// Botol
// Artinya:
// ini botol umum

// isinya belum ditentukan
// Lalu kamu pilih:
// Botol<Air>    //T bisa jadi Air
// Botol<Susu>   //T bisa jadi Susu
// Botol<Jus>    //T bisa jadi Jus

// Di kasus kamu:
// Channel<T> = jalur antrian umum
// Channel<ProductIndexMessage> = jalur antrian yang khusus membawa ProductIndexMessage
// </summary>
public class InMemoryProductIndexQueue : IProductIndexQueue
{
    private readonly Channel<ProductIndexMessage> _queue;

    public InMemoryProductIndexQueue()
    {
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _queue = Channel.CreateBounded<ProductIndexMessage>(options);
    }

    public async ValueTask EnqueueAsync(ProductIndexMessage message, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(message, cancellationToken);
    }

    public async ValueTask<ProductIndexMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}