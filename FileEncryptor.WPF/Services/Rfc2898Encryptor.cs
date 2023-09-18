using FileEncryptor.WPF.Services.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FileEncryptor.WPF.Services;

internal class Rfc2898Encryptor : IEncryptor
{
    private static readonly byte[] _Salt =
    {
        0x26, 0xdc, 0xff, 0x00,
        0xad, 0xed, 0x7a, 0xee,
        0xc5, 0xfe, 0x07, 0xaf,
        0x4d, 0x08, 0x22, 0x3c,
    };

    private static ICryptoTransform GetEncryptor(string password, byte[] salt = null)
    {
        var pdb = new Rfc2898DeriveBytes(password, salt ?? _Salt);
        var algorithm = Rijndael.Create();
        algorithm.Key = pdb.GetBytes(32);
        algorithm.IV = pdb.GetBytes(16);
        return algorithm.CreateEncryptor();
    }

    private static ICryptoTransform GetDecryptor(string password, byte[] salt = null)
    {
        var pdb = new Rfc2898DeriveBytes(password, salt ?? _Salt);
        var algorithm = Rijndael.Create();
        algorithm.Key = pdb.GetBytes(32);
        algorithm.IV = pdb.GetBytes(16);
        return algorithm.CreateDecryptor();
    }

    public void Encrypt(string soutcePath, string destonationPath, string password, int bufferLength = 104200)
    {
        var encryptor = GetEncryptor(password/*, Encoding.UTF8.GetBytes(soutcePath)*/);

        using var destinationEncrypted = File.Create(destonationPath, bufferLength);
        using var destination = new CryptoStream(destinationEncrypted, encryptor, CryptoStreamMode.Write);
        using var source = File.OpenRead(soutcePath);
        var buffer = new byte[bufferLength];
        int readed;
        do
        {
            Thread.Sleep(1);
            readed = source.Read(buffer, 0, bufferLength);
            destination.Write(buffer, 0, readed);
        }
        while(readed > 0);
        destination.FlushFinalBlock();
    }
    public bool Decrypt(string soutcePath, string destonationPath, string password, int bufferLength = 104200)
    {
        var decryptor = GetDecryptor(password);

        using var destinationDecrypted = File.Create(destonationPath, bufferLength);
        using var destination = new CryptoStream(destinationDecrypted, decryptor, CryptoStreamMode.Write);
        using var source = File.OpenRead(soutcePath);
        var buffer = new byte[bufferLength];
        int readed;
        do
        {
            readed = source.Read(buffer, 0, bufferLength);
            destination.Write(buffer, 0, readed);
        }
        while (readed > 0);

        try
        {
            destination.FlushFinalBlock();
        }
        catch(CryptographicException)
        {
            return false;
        }
        return true;
    }

    public async Task EncryptAsync(string soutcePath, string destonationPath, string password, int bufferLength = 104200, IProgress<double> progress = null, CancellationToken cancellation = default)
    {
        if(!File.Exists(soutcePath))
            throw new FileNotFoundException("Файл-источник для процесса шифрования не найден", soutcePath);
        if(bufferLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferLength), bufferLength, "Размер буфера чтения дожен быть больше 0");

        cancellation.ThrowIfCancellationRequested();

        var encryptor = GetEncryptor(password/*, Encoding.UTF8.GetBytes(soutcePath)*/);

        try
        {
            await using var destinationEncrypted = File.Create(destonationPath, bufferLength);
            await using var destination = new CryptoStream(destinationEncrypted, encryptor, CryptoStreamMode.Write);
            await using var source = File.OpenRead(soutcePath);

            var fileLength = source.Length;

            var buffer = new byte[bufferLength];
            int readed;
            do
            {
                Thread.Sleep(1);
                readed = await source.ReadAsync(buffer, 0, bufferLength, cancellation).ConfigureAwait(false);
                // дополнительные действия по завершению асинхронной операции
                await destination.WriteAsync(buffer, 0, readed, cancellation).ConfigureAwait(false);

                var filePosition = source.Position;
                progress?.Report((double)filePosition/fileLength);
                
                if (cancellation.IsCancellationRequested)
                {
                    //очистка состояния операции
                    cancellation.ThrowIfCancellationRequested();
                }
            }
            while (readed > 0);
            destination.FlushFinalBlock();
            progress?.Report(1);
        }
        catch(OperationCanceledException)
        {
            File.Delete(destonationPath);
            throw;
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error in EncryptAsync: {ex.Message}");
            throw;
        }
        
    }

    public async Task<bool> DecryptAsync(string soutcePath, string destonationPath, string password, int bufferLength = 104200, IProgress<double> progress = null, CancellationToken cancellation = default)
    {
        if (!File.Exists(soutcePath))
            throw new FileNotFoundException("Файл-источник для процесса дешифрования не найден", soutcePath);
        if (bufferLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferLength), bufferLength, "Размер буфера чтения дожен быть больше 0");

        cancellation.ThrowIfCancellationRequested();

        var decryptor = GetDecryptor(password);
        try
        {
            await using var destinationDecrypted = File.Create(destonationPath, bufferLength);
            await using var destination = new CryptoStream(destinationDecrypted, decryptor, CryptoStreamMode.Write);
            await using var source = File.OpenRead(soutcePath);

            var fileLength = source.Length;

            var buffer = new byte[bufferLength];
            int readed;
            do
            {
                readed = await source.ReadAsync(buffer, 0, bufferLength, cancellation).ConfigureAwait(false);
                await destination.WriteAsync(buffer, 0, readed, cancellation).ConfigureAwait(false);

                var filePosition = source.Position;
                progress?.Report((double)filePosition / fileLength);

                cancellation.ThrowIfCancellationRequested();
            }
            while (readed > 0);

            try
            {
                destination.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                return false;
            }
            
        }
        catch(OperationCanceledException)
        {
            File.Delete(destonationPath);
            throw;
        }

        progress?.Report(1);
        return true;
    }
}