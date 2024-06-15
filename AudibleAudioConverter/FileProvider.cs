using Windows.Storage.Pickers;

namespace AudibleAudioConverter;

public class FileProvider
{
    public async Task<string> ChooseFileAsync(string fileTypeFiler)
    {
        var fileOpenPicker = new FileOpenPicker();
        fileOpenPicker.FileTypeFilter.Add(fileTypeFiler);
        var file = await fileOpenPicker.PickSingleFileAsync();
        
        return file.Path;
    }
    
    public async Task<string> ChooseFolderAsync()
    {
        var folderPicker = new FolderPicker();
        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();
        
        return folder.Path;
    }
    
   
}
