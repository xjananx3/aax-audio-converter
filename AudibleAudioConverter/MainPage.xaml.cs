using System.Diagnostics;
using Windows.UI.Core;
using Aax.Activation.ApiClient;
using Uno.Disposables;

namespace AudibleAudioConverter;

public sealed partial class MainPage : Page
{
    private readonly FileProvider _fileProvider = new();
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
        StatusTextBlock.Visibility = Visibility.Visible;
        
        
        await Conversion();
        
    }
    
    private async Task Conversion()
    {
        var checksum = GetHash();
        var activationBytes = await GetBytes(checksum);
        var arguments = GetArguments(activationBytes);
        var totalDuration = await GetFileDurationAsync();
        await StartConversion(arguments, totalDuration);
    }
    
    private async Task StartConversion(string arguments, double totalDuration)
    {
        StatusTextBlock.Text = "Converting File...";
        ProgressBar.Value = 0;
        ProgressBar.Visibility = Visibility.Visible;
        
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
        process.ErrorDataReceived += (sender, e) => ProcessErrorData(e.Data, totalDuration);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await Task.Run(() => process.WaitForExitAsync());
        
        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            StopBar();
            EnableElements();
            StatusTextBlock.Text = "Conversion Completed";
            OpenOutputButton.Visibility = Visibility.Visible;
        });
    }
    
    private async void ProcessErrorData(string data, double totalDuration)
    {
        if(string.IsNullOrEmpty(data)) return;
        
        if (data.Contains("time="))
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                double percentage = ParseProgress(data, totalDuration);
                ProgressBar.Value = percentage;
            });
        }
    }
    
    private double ParseProgress(string data, double totalDuration)
    {
        try
        {
            var timeIndex = data.IndexOf("time=", StringComparison.Ordinal);
            if (timeIndex != -1)
            {
                var timeString = data.Substring(timeIndex + 5, 11);
                var timeSpan = TimeSpan.ParseExact(timeString, @"hh\:mm\:ss\.ff", null);
                
                if (totalDuration > 0)
                {
                    return timeSpan.TotalSeconds / totalDuration * 100;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing progress: {ex.Message}");
            
            return 0;
        }
        
        return 0;
    }
    
    private async Task<double> GetFileDurationAsync()
    {
        var tcs = new TaskCompletionSource<double>();
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{AaxFileDisplay.Text}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using (var process = new Process())
        {
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) =>
            {
                if (double.TryParse(e.Data, out double duration))
                {
                    tcs.SetResult(duration);
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            
            await process.WaitForExitAsync();
        }
        
        return await tcs.Task;
    }
    
    private void WriteToLog(string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            // Hier könntest du zusätzliche Ausgaben verarbeiten oder protokollieren
            Debug.WriteLine($"ffmpeg Output: {data}");
        }
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
        
        var checksum = HashExtractor.GetActivationChecksum(AaxFileDisplay.Text);
        
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
