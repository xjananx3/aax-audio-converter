using System.Text;

namespace AudibleAudioConverter;

public static class ActivationByteHashExtractor
{
    public static string GetActivationChecksum(string path)
    {
        using (var fs = File.OpenRead(path))
        using (var br = new BinaryReader(fs))
        {
            fs.Position = 0x251 + 56 + 4;
            var checksum = br.ReadBytes(20);
            return checksum.ToHexString();
        }
    }
    
    private static string ToHexString(this byte[] source)
    {
        return source.ToHexString(0, source.Length);
    }
    
    private static string ToHexString(this byte[] source, int offset, int length)
    {
        StringBuilder stringBuilder = new StringBuilder(length * 2);
        for (int i = offset; i < offset + length; i++)
        {
            stringBuilder.AppendFormat("{0:x2}", source[i]);
        }
        
        return stringBuilder.ToString();
    }
}
