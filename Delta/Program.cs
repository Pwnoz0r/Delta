using System;
using System.IO;
using DeltaPatching;

/* 
 * This is a play around project for development testing and by no means reflects the final product.
*/

namespace Delta
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-dev":
                            if (args.Length >= i)
                            {
                                foreach (var s in args)
                                    Console.WriteLine("ARG: " + s);

                                if (!string.IsNullOrEmpty(args[0]))
                                    if (Directory.Exists(args[0]))
                                        Hash.GenerateHashFilesInDirectory(args[0], Hash.DirectoryOutput);
                                    else
                                        foreach (var s in Hash.CompareHash(args[0], args[1]))
                                            Console.WriteLine($"{s.Key}^{s.Value}");
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(args[0]))
                                if (Directory.Exists(args[0]))
                                    Hash.GenerateHashFilesInDirectory(args[0], Hash.DirectoryOutput, "*", true);
                            break;
                    }
                }
            }

            /* Example usage of CompareHash
            foreach (KeyValuePair<string, string> s in Hash.CompareHash(localFilePath, remoteFileURL))
                Console.WriteLine(string.Format("{0}^{1}", s.Key, s.Value));
            */

            Console.WriteLine("Finished... press any key to continue!");
            Console.ReadKey();
        }
    }
}
