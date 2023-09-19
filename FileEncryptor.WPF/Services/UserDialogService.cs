using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.Views.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FileEncryptor.WPF.Services;

internal class UserDialogService : IUserDialog
{
    public bool OpenFile(string title, out string selectedFile, string filter = "Все файлы (*.*)|*.*")
    {
        var fileDialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
        };

        if(fileDialog.ShowDialog() != true)
        {
            selectedFile = null;
            return false;
        }

        selectedFile = fileDialog.FileName;
        return true;
    }

    public bool OpenFiles(string title, out IEnumerable<string> selectedFiles, string filter = "Все файлы (*.*)|*.*")
    {
        var fileDialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
        };

        if (fileDialog.ShowDialog() != true)
        {
            selectedFiles = Enumerable.Empty<string>();
            return false;
        }

        selectedFiles = Enumerable.Empty<string>();
        return true;
    }

    public bool SaveFile(string title, out string selectedFile, string defaultFileName = null, string filter = "Все файлы (*.*)|*.*")
    {
        var fileDialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter,
        };

        if(!string.IsNullOrWhiteSpace(defaultFileName))
            fileDialog.FileName = defaultFileName;

        if (fileDialog.ShowDialog() != true)
        {
            selectedFile = null;
            return false;
        }

        selectedFile = fileDialog.FileName;
        return true;
    }

    public void Error(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    public void Warning(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    public void Information(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    (IProgress<double> Progress, IProgress<string> Status, CancellationToken Cancel, Action Close) IUserDialog.ShowProgress(string title)
    {
        var progressWindow = new ProgressWindow { Title = title, Owner = App.FocusedWindow, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        progressWindow.Show();
        return (progressWindow.ProgressInformer, progressWindow.StatusInformer, progressWindow.Cancellation, progressWindow.Close);
    }
}