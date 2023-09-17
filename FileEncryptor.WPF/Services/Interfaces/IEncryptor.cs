namespace FileEncryptor.WPF.Services.Interfaces;

internal interface IEncryptor
{
    void Encrypt(string soutcePath, string destonationPath, string password, int bufferLength = 104200);
    bool Decrypt(string soutcePath, string destonationPath, string password, int bufferLength = 104200);

}