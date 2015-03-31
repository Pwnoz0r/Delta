using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeltaPatching;
using DeltaPatching.Util;
using System.IO;

/* 
 * This is a play around project for development testing and by no means reflects the final product.
*/

namespace Delta
{
    class Program
    {
        public static Config _Config = new Config();
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-dev":
                            if (args.Length >= i)
                            {
                                foreach (string s in args)
                                    Console.WriteLine("ARG: " + s);

                                if (!string.IsNullOrEmpty(args[0]))
                                    if (Directory.Exists(args[0]))
                                        Hash.GenerateHashFilesInDirectory(args[0], Config.DirectoryOutput);
                                    else
                                        foreach (KeyValuePair<string, string> s in Hash.CompareHash(args[0], args[1]))
                                            Console.WriteLine(string.Format("{0}^{1}", s.Key, s.Value));
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(args[0]))
                                if (Directory.Exists(args[0]))
                                    Hash.GenerateHashFilesInDirectory(args[0], Config.DirectoryOutput);
                            break;
                    }
                }
            }

            /* Example usage of CompareHash
            foreach (KeyValuePair<string, string> s in Hash.CompareHash(localFilePath, remoteFileURL))
                Console.WriteLine(string.Format("{0}^{1}", s.Key, s.Value));
            */

            // START DEBUG
            Console.WriteLine("Finished... press any key to continue!");
            Console.ReadKey();
            // END DEBUG
        }
    }
}
