using Aax.Activation.ApiClient;
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
        
        extractButton.IsEnabled = true;
        
        fop.TryDispose();
    }
    
    private async void ExtractButton_Click(object sender, RoutedEventArgs e)
    {
        var fop = new FileProvider();
        fop.FolderPicker.FileTypeFilter.Add("*");
        
        var storageFolder = await fop.FolderPicker.PickSingleFolderAsync();
        extractFileDisplay.Text = storageFolder.Path;
        
        convertButton.IsEnabled = true;
        
        fop.TryDispose();
    }
    
    private async void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        DisableElements();
        
        await StartConversion();
        
    }
    
    private async Task StartConversion()
    {
        var checksum = GetHash();
        var activationBytes = await GetBytes(checksum);
        string arguments = GetArguments(activationBytes);
    }
    
    private string GetArguments(object activationBytes)
    {
        string fileout = Path.Combine(extractFileDisplay.Text, Path.GetFileNameWithoutExtension(aaxFileDisplay.Text) + GetOutExtension());
        
        string arg = @"-y -activation_bytes ";
        string arg1 = @" -i ";
        string arg2 = @" -ab ";
        string arg3 = @"k -map_metadata 0 -id3v2_version 3 -vn ";
        
        string arguments = arg + activationBytes + arg1 + aaxFileDisplay.Text + arg3 + fileout;
        
        return arguments;
    }
    
    private string GetOutExtension()
    {
        return ".mp3";
    }
    
    private async Task<object> GetBytes(string checksum)
    {
        var activationBytes = await AaxActivationClient.Instance.ResolveActivationBytes(checksum);
        
        return activationBytes;
    }
    
    private string GetHash()
    {
        var checksum = ActivationByteHashExtractor.GetActivationChecksum(aaxFileDisplay.Text);
        
        return checksum;
    }
    
    private void DisableElements()
    {
        //Buttons
        chooseAaxButton.IsEnabled = false;
        extractButton.IsEnabled = false;
        convertButton.IsEnabled = false;
        
        //Textboxes
        aaxFileDisplay.IsEnabled = false;
        extractFileDisplay.IsEnabled = false;
    }
}
