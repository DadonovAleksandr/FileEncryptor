using Microsoft.Extensions.DependencyInjection;

namespace FileEncryptor.WPF.ViewModels;

internal class ViewModelLocator
{
    public MainWindowViewModel MainWindowViewModel => App.Services.GetRequiredService<MainWindowViewModel>();
}