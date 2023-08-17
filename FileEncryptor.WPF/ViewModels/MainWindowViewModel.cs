using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;
using System;
using System.IO;
using System.Windows.Input;

namespace FileEncryptor.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    private readonly IUserDialog _userDialog;

    public MainWindowViewModel(IUserDialog userDialog)
    {
        _userDialog = userDialog;
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
    private ICommand _SelectedFileCommand;
    public ICommand SelectedFileCommand => _SelectedFileCommand ??= new LambdaCommand(OnSelectedFileCommandExecuted);
    private void OnSelectedFileCommandExecuted()
    {
        if (!_userDialog.OpenFile("Выбор файла для шифрования", out var filePath)) return;
        SelectedFile = new FileInfo(filePath);
    }

    private ICommand _EncryptCommand;
    public ICommand EncryptCommand => _EncryptCommand ??= new LambdaCommand(OnEncryptCommandExecuted, CanEncryptCommandExecute);

    private bool CanEncryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

    private void OnEncryptCommandExecuted(object p)
    {
        var file = p as FileInfo ?? SelectedFile;
        if (file is null) return;

        SelectedFile = new FileInfo(filePath);
    }

    private ICommand _DecryptCommand;
    public ICommand DecryptCommand => _DecryptCommand ??= new LambdaCommand(OnDecryptCommandExecuted, CanDecryptCommandExecute);

    private bool CanDecryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

    private void OnDecryptCommandExecuted(object p)
    {
        if (!_userDialog.OpenFile("Выбор файла для шифрования", out var filePath)) return;
        SelectedFile = new FileInfo(filePath);
    }

    #endregion


}