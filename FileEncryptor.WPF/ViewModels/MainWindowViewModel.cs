using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Infrastructure.Commands.Base;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;

namespace FileEncryptor.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    private const string _EncryptedFileSuffix = ".encrypted";
    private readonly IUserDialog _userDialog;
    private readonly IEncryptor _encryptor;
    private CancellationTokenSource _processCancellation;

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

    #region Progress
    private double _progressValue;
    public double ProgressValue
    {
        get { return _progressValue; }
        set { Set(ref _progressValue, value); }
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

    private async void OnEncryptCommandExecuted(object p)
    {
        var file = p as FileInfo ?? SelectedFile;
        if (file is null) return;
        
        var defaultFileName = file.FullName + _EncryptedFileSuffix;
        if (!_userDialog.SaveFile("Выбор файла для сохранения", out var destinationPath, defaultFileName)) return;

        var timer = Stopwatch.StartNew();
        var progress = new Progress<double>(p => ProgressValue = p);

        _processCancellation = new CancellationTokenSource();
        
        ((Command)EncryptCommand).Executable = false;
        ((Command)DecryptCommand).Executable = false;
        var encryption_task = _encryptor.EncryptAsync(file.FullName, destinationPath, Password, progress: progress, cancellation: _processCancellation.Token);
        try
        {
            await encryption_task;
        }
        catch (OperationCanceledException) { } 
        finally
        {
            _processCancellation.Dispose();
            _processCancellation = null;
        }
        ((Command)EncryptCommand).Executable = true;
        ((Command)DecryptCommand).Executable = true;
        timer.Stop();
        //_userDialog.Information("Шифрование", $"Шифрование файла успешно завершено за {timer.Elapsed.TotalSeconds:0.##} c");
    }
    #endregion

    #region DecryptCommand
    private ICommand _DecryptCommand;
    public ICommand DecryptCommand => _DecryptCommand ??= new LambdaCommand(OnDecryptCommandExecuted, CanDecryptCommandExecute);

    private bool CanDecryptCommandExecute(object p) => (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

    private async void OnDecryptCommandExecuted(object p)
    {
        var file = p as FileInfo ?? SelectedFile;
        if (file is null) return;

        var defaultFileName = file.FullName.EndsWith(_EncryptedFileSuffix) 
            ? file.FullName.Substring(0, file.FullName.Length - _EncryptedFileSuffix.Length) 
            : file.FullName;
        if (!_userDialog.SaveFile("Выбор файла для сохранения", out var destinationPath, defaultFileName)) return;

        var timer = Stopwatch.StartNew();
        ((Command)EncryptCommand).Executable = false;
        ((Command)DecryptCommand).Executable = false;

        var progress = new Progress<double>(p => ProgressValue = p);
        
        _processCancellation = new CancellationTokenSource();

        var descryption_task = _encryptor.DecryptAsync(file.FullName, destinationPath, Password, progress: progress, cancellation: _processCancellation.Token);
        // дополнительный код, выполняемый параллельно процессу дешифровки
        var success = false;
        try
        {
            success = await descryption_task;
        }
        catch(OperationCanceledException) { }
        finally
        {
            _processCancellation.Dispose();
            _processCancellation = null;
        }

        ((Command)EncryptCommand).Executable = true;
        ((Command)DecryptCommand).Executable = true;
        timer.Stop();
        if (success)
            _userDialog.Information("Шифрование", $"Дешифровка файла выпонена успешно за {timer.Elapsed.TotalSeconds:0.##} с");
        else
            _userDialog.Warning("Шифрование", $"Ошибка при дешифровке файла: указан неверный пароль") ;
    }
    #endregion

    #region CancelCommand
    private ICommand _CancelCommand;
    public ICommand CancelCommand => _CancelCommand ??= new LambdaCommand(OnCancelCommandExecuted, CanCancelCommandExecute);

    private bool CanCancelCommandExecute(object p) => _processCancellation != null && !_processCancellation.IsCancellationRequested;

    private async void OnCancelCommandExecuted(object p) => _processCancellation.Cancel();

    #endregion

    #endregion


}