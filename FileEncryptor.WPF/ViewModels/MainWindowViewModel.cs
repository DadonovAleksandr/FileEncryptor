using FileEncryptor.WPF.ViewModels.Base;

namespace FileEncryptor.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    #region Title
    private string _title = "Шифратор";
    public string Title
    {
        get { return _title; }
        set { Set(ref _title, value); }
    }
    #endregion
}