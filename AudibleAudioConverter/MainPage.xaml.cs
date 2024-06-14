using System.Diagnostics;
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
        
        await StartConversion();
        
    }
    
    private async Task StartConversion()
    {
        var checksum = GetHash();
        var activationBytes = await GetBytes(checksum);
        var arguments = GetArguments(activationBytes);
        await ConvertingFileProcess(arguments);
    }
    
    private async Task ConvertingFileProcess(string arguments)
    {
        StatusTextBlock.Text = "Converting File...";
        
        Process ffmProcess = new Process();
        ffmProcess.StartInfo.FileName = "ffmpeg";
        ffmProcess.StartInfo.Arguments = arguments;
        ffmProcess.StartInfo.CreateNoWindow = true;
        ffmProcess.StartInfo.RedirectStandardError = true;
        ffmProcess.StartInfo.UseShellExecute = false;
        ffmProcess.EnableRaisingEvents = true;
        
        ffmProcess.Start();
        
        ffmProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogTextBox.Text.Append(e.Data + Environment.NewLine);
            }
        };
        
        ffmProcess.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogTextBox.Text.Append(e.Data + Environment.NewLine);
            }
        };
        
        ffmProcess.BeginErrorReadLine();
        ffmProcess.BeginOutputReadLine();
        await ffmProcess.WaitForExitAsync();
        
        ffmProcess.Close();
        
        ChooseAaxButton.IsEnabled = true;
        ConvertButton.IsEnabled = true;
        
        StatusTextBlock.Text = "Conversion Complete!";
        
    }
    
    private string GetArguments(object activationBytes)
    {
        StatusTextBlock.Text = "Getting Arguments";
        
        string fileout = Path.Combine(ExtractFolderDisplay.Text, Path.GetFileNameWithoutExtension(AaxFileDisplay.Text) + GetOutExtension());
        string quality = GetSelectedQuality();
        
        string arg = @"-y -activation_bytes ";
        string arg1 = @" -i ";
        string arg2 = @" -ab ";
        string arg3 = @"k -map_metadata 0 -id3v2_version 3 -vn ";
        
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
            case 0: return "mp3";
            case 1: return "m4b";
            case 2: return "flac";
            default: return "mp3";
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
