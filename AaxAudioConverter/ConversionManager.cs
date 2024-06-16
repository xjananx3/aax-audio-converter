using System.Data;
using System.Diagnostics;
using Aax.Activation.ApiClient;

namespace AudibleAudioConverter;

public class ConversionManager
{
    private Process _conversionProcess;
    private CancellationTokenSource _cancellationTokenSource;
    
    public async Task StartConversionAsync(
        string aaxFilePath,
        string extractFolderPath,
        string format,
        string quality,
        Action<double> updateProgress,
        Action<string> updateStatus)
    {
        _cancellationTokenSource= new CancellationTokenSource();
        
        updateStatus("Getting File Hash");
        var checksum = HashExtractor.GetActivationChecksum(aaxFilePath);
        updateStatus("Getting Activation bytes");
        var activationBytes = await AaxActivationClient.Instance.ResolveActivationBytes(checksum);
        
        var arguments = $"-y -activation_bytes {activationBytes} -i {aaxFilePath} -ab {quality} -map_metadata 0 -id3v2_version 3 -vn {Path.Combine(extractFolderPath, Path.GetFileNameWithoutExtension(aaxFilePath) + format)}";
        var totalDuration = await GetFileDurationAsync(aaxFilePath);
        
        updateStatus("Converting File...");
        await StartProcessAsync(arguments, totalDuration, updateProgress, updateStatus, _cancellationTokenSource.Token);
    }
    
    private async Task StartProcessAsync(
        string arguments, 
        double totalDuration, 
        Action<double> updateProgress, 
        Action<string> updateStatus,
        CancellationToken cancellationToken)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        
        _conversionProcess = new Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
        
        _conversionProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data) && e.Data.Contains("time="))
            {
                var percentage = ParseProgress(e.Data, totalDuration);
                updateProgress(percentage);
            }
        };
        
        _conversionProcess.Start();
        _conversionProcess.BeginErrorReadLine();
        
        try
        {
            await Task.Run(() => _conversionProcess.WaitForExitAsync(), cancellationToken);
            
            if (!cancellationToken.IsCancellationRequested)
            {
                updateStatus("Conversion Completed");
            }
            
        }
        catch (OperationCanceledException)
        {
            updateStatus("Conversion Canceled");
        }
        finally
        {
            _conversionProcess?.Dispose();
            _conversionProcess = null;
        }
        
    }
    
    public void CancelConversion()
    {
        if (_conversionProcess != null && !_conversionProcess.HasExited)
        {
            _cancellationTokenSource?.Cancel();
            _conversionProcess.Kill();
            _conversionProcess.Dispose();
            _conversionProcess = null;
        }
    }
    
    private async Task<double> GetFileDurationAsync(string filePath)
    {
        var tcs = new TaskCompletionSource<double>();
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
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
    
    
}
