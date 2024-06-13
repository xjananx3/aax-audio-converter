using Uno.Disposables;

namespace AudibleAudioConverter;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
    }
    
    private async void ChooseAaxButton_Click(object sender, RoutedEventArgs e)
    {
       
        var fop = new FileProvider();
        fop.FileOpenPicker.FileTypeFilter.Add(".aax");
            
        var storageFile = await fop.FileOpenPicker.PickSingleFileAsync();
        aaxFileDisplay.Text = storageFile.Path;    
        
        fop.TryDispose();
    }
    
    private async void ExtractButton_Click(object sender, RoutedEventArgs e)
    {
        var fop = new FileProvider();
        fop.FolderPicker.FileTypeFilter.Add("*");
        
        var storageFolder = await fop.FolderPicker.PickSingleFolderAsync();
        extractFileDisplay.Text = storageFolder.Path;
        
        fop.TryDispose();
    }
}
