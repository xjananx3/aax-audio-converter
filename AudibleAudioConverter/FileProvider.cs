using Windows.Storage.Pickers;

namespace AudibleAudioConverter;

public class FileProvider
{
    public FileOpenPicker FileOpenPicker { get; set; } = new()
    {
        
        SuggestedStartLocation = PickerLocationId.ComputerFolder
        
    };
    
    public FolderPicker FolderPicker { get; set; } = new()
    {
        
        SuggestedStartLocation = PickerLocationId.ComputerFolder
        
    };
}
