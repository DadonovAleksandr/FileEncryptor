﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileEncryptor.WPF.Services.Interfaces;

internal interface IEncryptor
{
    void Encrypt(string soutcePath, string destonationPath, string password, int bufferLength = 104200);
    bool Decrypt(string soutcePath, string destonationPath, string password, int bufferLength = 104200);


    Task EncryptAsync(string soutcePath, string destonationPath, string password, int bufferLength = 104200, IProgress<double> progress = null, CancellationToken cancellation = default);
    Task<bool> DecryptAsync(string soutcePath, string destonationPath, string password, int bufferLength = 104200, IProgress<double> progress = null, CancellationToken cancellation = default);
}