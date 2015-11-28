using System.IO;

namespace DeltaPatching.Util
{
    public class Config
    {
        public static string DirectoryCurrent = Directory.GetCurrentDirectory();
        public static string DirectoryOutput = Path.Combine(DirectoryCurrent, "Output");
    }
}
