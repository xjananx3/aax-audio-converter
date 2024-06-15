using System.Diagnostics;
using Windows.UI.Core;
using Aax.Activation.ApiClient;
using Uno.Disposables;

namespace AudibleAudioConverter;

public sealed partial class MainPage : Page
{
    private readonly FileProvider _fileProvider = new();
    private readonly ConversionManager _conversionManager = new();
    public MainPage()
    {
        InitializeComponent();
    }
    
    private async void ChooseAaxButton_Click(object sender, RoutedEventArgs e)
    {
        
        var filePath = await _fileProvider.ChooseFileAsync(".aax");
        AaxFileDisplay.Text = filePath;
        ExtractFolderButton.IsEnabled = true;
    }
    
    private async void ExtractButton_Click(object sender, RoutedEventArgs e)
    {
        var folderPath = await _fileProvider.ChooseFolderAsync();
        ExtractFolderDisplay.Text = folderPath;
        
        ConvertButton.IsEnabled = true;
    }
    
    private async void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        DisableElements();
        StartBar();
        
        await _conversionManager.StartConversionAsync(
            AaxFileDisplay.Text,
            ExtractFolderDisplay.Text,
            GetSelectedFormat(),
            GetSelectedQuality(),
            UpdateProgressBar,
            ShowStatus);
        
        EnableElements();
        StopBar();
        OpenOutputButton.Visibility = Visibility.Visible;
    }
    
    private void ShowStatus(string message)
    {
        StatusTextBlock.Text = message;
    }
    
    private void UpdateProgressBar(double percentage)
    {
        ProgressBar.Value = percentage;
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
    
    private string GetSelectedFormat()
    {
        switch (FormatComboBox.SelectedIndex)
        {
            case 0: return ".mp3";
            case 1: return ".m4b";
            case 2: return ".flac";
            default: return ".mp3";
        }
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
        ChooseAaxButton.IsEnabled = true;
        
        AaxFileDisplay.IsEnabled = true;
        ExtractFolderDisplay.IsEnabled = true;
        
        FormatComboBox.IsEnabled = true;
        QualityComboBox.IsEnabled = true;
    }
    
    private void OpenOutputButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string outputFolderPath = ExtractFolderDisplay.Text;
            
            if (Directory.Exists(outputFolderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputFolderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                StatusTextBlock.Text = "Output folder not found";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening output folder: {ex.Message}");
            StatusTextBlock.Text = "Error occurred while opening the output folder";
        }
    }
    
    private void StartBar()
    {
        ProgressBar.IsIndeterminate = true;
    }
    
    private void StopBar()
    {
        ProgressBar.IsIndeterminate = false;
        ProgressBar.Value = 0.1;
    }
}
