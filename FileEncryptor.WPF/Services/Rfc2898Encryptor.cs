using FileEncryptor.WPF.Services.Interfaces;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

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
}