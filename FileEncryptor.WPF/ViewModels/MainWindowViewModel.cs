﻿using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace FileEncryptor.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    private const string _EncryptedFileSuffix = ".encrypted";
    private readonly IUserDialog _userDialog;
    private readonly IEncryptor _encryptor;

    public MainWindowViewModel(IUserDialog userDialog, IEncryptor encryptor)
    {
        _userDialog = userDialog;
        _encryptor = encryptor;
    }

    #region Title
    private string _title = "Шифратор";
    public string Title
    {
        get { return _title; }
        set { Set(ref _title, value); }
    }
    #endregion


    #region Password
    private string _Password = "123";
    public string Password
    {
        get { return _Password; }
        set { Set(ref _Password, value); }
    }
    #endregion

    #region SelectedFile
    private FileInfo _SelectedFile;
    public FileInfo SelectedFile
    {
        get { return _SelectedFile; }
        set { Set(ref _SelectedFile, value); }
    }
    #endregion

    #region Команды
    #region SelectedFileCommand
    private ICommand _SelectedFileCommand;
    public ICommand SelectedFileCommand => _SelectedFileCommand ??= new LambdaCommand(OnSelectedFileCommandExecuted);
    private void OnSelectedFileCommandExecuted()
    {
        if (!_userDialog.OpenFile("Выбор файла для шифрования", out var filePath)) 
            return;
        var selectedFile = new FileInfo(filePath);
        SelectedFile = selectedFile.Exists ? selectedFile : null;
    }
    #endregion

    #region EncryptCommand
    private ICommand _EncryptCommand;
    public ICommand EncryptCommand => _EncryptCommand ??= new LambdaCommand(OnEncryptCommandExecuted, CanEncryptCommandExecute);

    private bool CanEncryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

    private void OnEncryptCommandExecuted(object p)
    {
        var file = p as FileInfo ?? SelectedFile;
        if (file is null) return;
        
        var defaultFileName = file.FullName + _EncryptedFileSuffix;
        if (!_userDialog.SaveFile("Выбор файла для сохранения", out var destinationPath, defaultFileName)) return;

        var timer = Stopwatch.StartNew();
        _encryptor.Encrypt(file.FullName, destinationPath, Password);
        timer.Stop();
        _userDialog.Information("Шифрование", $"Шифрование файла успешно завершено за {timer.Elapsed.TotalSeconds:0.##} c");
    }
    #endregion

    #region DecryptCommand
    private ICommand _DecryptCommand;
    public ICommand DecryptCommand => _DecryptCommand ??= new LambdaCommand(OnDecryptCommandExecuted, CanDecryptCommandExecute);

    private bool CanDecryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

    private void OnDecryptCommandExecuted(object p)
    {
        var file = p as FileInfo ?? SelectedFile;
        if (file is null) return;

        var defaultFileName = file.FullName.EndsWith(_EncryptedFileSuffix) 
            ? file.FullName.Substring(0, file.FullName.Length - _EncryptedFileSuffix.Length) 
            : file.FullName;
        if (!_userDialog.SaveFile("Выбор файла для сохранения", out var destinationPath, defaultFileName)) return;

        var timer = Stopwatch.StartNew();
        var success =_encryptor.Decrypt(file.FullName, destinationPath, Password);
        timer.Stop();
        if (success)
            _userDialog.Information("Шифрование", $"Дешифровка файла выпонена успешно за {timer.Elapsed.TotalSeconds:0.##} с");
        else
            _userDialog.Warning("Шифрование", $"Ошибка при дешифровке файла: указан неверный пароль") ;
    }
    #endregion

    #endregion


}