using System.Diagnostics;
using Windows.UI.Core;
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
        AaxFileDisplay.Text = storageFile.Path;
        
        ExtractFolderButton.IsEnabled = true;
        
        fop.TryDispose();
    }
    
    private async void ExtractButton_Click(object sender, RoutedEventArgs e)
    {
        var fop = new FileProvider();
        fop.FolderPicker.FileTypeFilter.Add("*");
        
        var storageFolder = await fop.FolderPicker.PickSingleFolderAsync();
        ExtractFolderDisplay.Text = storageFolder.Path;
        
        ConvertButton.IsEnabled = true;
        
        fop.TryDispose();
    }
    
    private async void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        DisableElements();
        StatusTextBlock.Visibility = Visibility.Visible;
        
        await Conversion();
        
    }
    
    private async Task Conversion()
    {
        var checksum = GetHash();
        var activationBytes = await GetBytes(checksum);
        var arguments = GetArguments(activationBytes);
        await StartConversion(arguments);
    }
    
    private async Task StartConversion(string arguments)
    {
        StatusTextBlock.Text = "Converting File...";
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        
        Process process = new Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
        
        process.OutputDataReceived += (sender, e) => WriteToLog(e.Data);
        process.ErrorDataReceived += (sender, e) => WriteToLog(e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await Task.Run(() => process.WaitForExitAsync());
        
        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            StatusTextBlock.Text = "Conversion Completed";
            OpenOutputButton.Visibility = Visibility.Visible;
        });
        
        EnableElements();
        
    }
    
    private void WriteToLog(string data)
    {
        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            if (!string.IsNullOrEmpty(data))
            {
                //LogTextBox.Text.Append(data + Environment.NewLine);
            }
        });
    }
    
    private string GetArguments(object activationBytes)
    {
        StatusTextBlock.Text = "Getting Arguments";
        
        string fileout = Path.Combine(ExtractFolderDisplay.Text, Path.GetFileNameWithoutExtension(AaxFileDisplay.Text) + GetOutExtension());
        string quality = GetSelectedQuality();
        
        string arg = @"-y -activation_bytes ";
        string arg1 = @" -i ";
        string arg2 = @" -ab ";
        string arg3 = @" -map_metadata 0 -id3v2_version 3 -vn ";
        
        string arguments = arg + activationBytes + arg1 + AaxFileDisplay.Text + arg2 + quality + arg3 + fileout;
        
        return arguments;
    }
    
    
    private string GetSelectedQuality()
    {
        switch (QualityComboBox.SelectedIndex)
        {
            case 0: return "32k"; //Very Low
            case 1: return "80k";
            case 2: return "160k"; //Medium
            case 3: return "256k"; 
            case 4: return "320k";  //Very High
            default: return "96k"; 
        }
    }
    
    private string GetOutExtension()
    {
        switch (FormatComboBox.SelectedIndex)
        {
            case 0: return ".mp3";
            case 1: return ".m4b";
            case 2: return ".flac";
            default: return ".mp3";
        }
    }
    
    private async Task<string> GetBytes(string checksum)
    {
        StatusTextBlock.Text = "Getting Activation bytes";
        
        var activationBytes = await AaxActivationClient.Instance.ResolveActivationBytes(checksum);
        
        return activationBytes;
    }
    
    private string GetHash()
    {
        StatusTextBlock.Text = "Getting File Hash";
        
        var checksum = ActivationByteHashExtractor.GetActivationChecksum(AaxFileDisplay.Text);
        
        return checksum;
    }
    
    private void DisableElements()
    {
        //Buttons
        ChooseAaxButton.IsEnabled = false;
        ExtractFolderButton.IsEnabled = false;
        ConvertButton.IsEnabled = false;
        
        //Textboxes
        AaxFileDisplay.IsEnabled = false;
        ExtractFolderDisplay.IsEnabled = false;
        
        FormatComboBox.IsEnabled = false;
        QualityComboBox.IsEnabled = false;
    }
    
    private void EnableElements()
    {
        ChooseAaxButton.IsEnabled = false;
        
        AaxFileDisplay.IsEnabled = false;
        ExtractFolderDisplay.IsEnabled = false;
        
        FormatComboBox.IsEnabled = false;
        QualityComboBox.IsEnabled = false;
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
    
    private void OpenOutputButton_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
