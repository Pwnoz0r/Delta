using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaPatching.Util
{
    public class Config
    {
        public static string DirectoryCurrent = Directory.GetCurrentDirectory();
        public static string DirectoryOutput = Path.Combine(DirectoryCurrent, "Output");
    }
}
