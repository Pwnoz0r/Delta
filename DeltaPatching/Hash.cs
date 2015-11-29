/*
 * @author: Jonathan "Pwnoz0r" Rainier
 * @description: Compare generated local and remote differences and return the results into a Dictionary (KeyValuePair);
 * @license: Copyright 2015 Jonathan "Pwnoz0r" Rainier
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using System.Text.RegularExpressions;
// ReSharper disable LoopCanBeConvertedToQuery

namespace DeltaPatching
{
    public class Hash
    {
        public static string DirectoryCurrent => Directory.GetCurrentDirectory();
        public static string DirectoryOutput = Path.Combine(DirectoryCurrent, "Output");

        /// <summary>
        /// Generate a hash from the given file path.
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <param name="outputFile">Create a (filename).md5 file when generating the hash.</param>
        /// <returns>Filename and hash in the following format: "filename:hash"</returns>
        public static string GenerateHash(string file, bool outputFile = false)
        {
            string computedHash;

            using (var md5 = MD5.Create())
                using (var fileStream = File.OpenRead(file))
                    computedHash = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLower();

            Console.WriteLine($"{Path.GetFileName(file)}:{computedHash}");

            if (!outputFile) return computedHash;
            using (var sw = new StreamWriter(Path.Combine(DirectoryOutput, $"{Path.GetFileName(file)}.md5")))
                sw.Write(computedHash);

            return computedHash;
        }

        /// <summary>
        /// Generate hash of a given directory.
        /// </summary>
        /// <param name="path">Directory to be hashed</param>
        /// <param name="outputFolder">Directory that the hash.txt file will write to.</param>
        /// <param name="fileFilter">File filter. * is an all file wildcard (default): fileFilter = "*.dll". Multiple wildcards can be used: fileFilter = "*.exe,*.dll"</param>
        /// <param name="includeSubDirectories">Boolean to include perform hashing recursively.</param>
        /// <returns>Compiled list of all file names and their hashes.</returns>
        public static List<string> GenerateHashFilesInDirectory(string path, string outputFolder, string fileFilter = "*", bool includeSubDirectories = false)
        {
            DirectoryOutput = path;
            if (!Directory.Exists(DirectoryOutput))
                Directory.CreateDirectory(DirectoryOutput);

            var files = includeSubDirectories ? (Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)).ToList() : (Directory.GetFiles(path)).ToList();
            var compiledList = new List<string>();

            if (path != null && path.Contains('\\'))
            {
                var index = path.LastIndexOf(@"\", StringComparison.Ordinal);
                if (index != -1)
                    path = path.Substring(index).TrimStart('\\');
            }

            var rootPath = path;

            foreach (var fileName in files)
            {
                if (path == null) continue;

                var folderStructure = Regex.Split(fileName, rootPath)[1];
                var case1 = $"{Path.GetFileName(fileName)}";
                var case2 = $@"{Regex.Split(fileName, rootPath)[1].TrimStart('\\')}";

                if (fileFilter == "*")
                {
                    var set = folderStructure.Trim('\\') == Path.GetFileName(fileName) ? $"{case1}:{GenerateHash(fileName)}" : $"{case2}:{GenerateHash(fileName)}";
                    compiledList.Add(set);
                }
                else
                {
                    foreach(var filter in fileFilter.Split(','))
                    {
                        var extension = Path.GetExtension(fileName);

                        if (extension == null || !extension.Contains(filter.TrimStart('*'))) continue;

                        var set = folderStructure.Trim('\\') == Path.GetFileName(fileName) ? $"{case1}:{GenerateHash(fileName)}" : $"{case2}:{GenerateHash(fileName)}";

                        compiledList.Add(set);
                    }
                }
            }

            var hashListPath = Path.Combine(outputFolder, $"{path}.txt");

            if (File.Exists(hashListPath))
                File.Delete(hashListPath);

            var rawPath = Path.GetDirectoryName(hashListPath);
            if (rawPath != null && !Directory.Exists(rawPath))
                Directory.CreateDirectory(rawPath);

            foreach (var s in compiledList)
                using (var sw = new StreamWriter(hashListPath, true))
                    sw.WriteLine(s);

            return compiledList;
        }

        /// <summary>
        /// Compare two files containg hashes
        /// </summary>
        /// <param name="localHashFile">File path to your local hash file.</param>
        /// <param name="remoteHashFile">URL (preferably *.txt) to your remote hash file.</param>
        /// <returns>List containing strings of the two file differences.</returns>
        public static Dictionary<string, string> CompareHash(string localHashFile, string remoteHashFile)
        {
            var d1 = new List<string>();
            var d2 = new List<string>();
            var allDiff = new Dictionary<string, string>();

            using (var r = new StringReader(GetLocalHash(localHashFile)))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    d1.Add(line);
            }

            using (var r = new StringReader(GetRemoteHash(remoteHashFile)))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    d2.Add(line);
            }

            var remoteDifferences = d2.Except(d1).ToList();
            var clientDifferences = d1.Except(d2).ToList();

            // R = REMOTE : C = CLIENT
            foreach (var s in remoteDifferences)
                allDiff.Add(s, "R");
            foreach (var s in clientDifferences)
                allDiff.Add(s, "C");

            return allDiff;
        }

        public static string GetLocalHash(string file)
        {
            if (!File.Exists(file))
                File.Create(file).Close();
            return File.ReadAllText(file);
        }

        public static string GetRemoteHash(string url)
        {
            string contentsRemote;
            using (var wc = new WebClient())
            {
                wc.Proxy = null;
                contentsRemote = wc.DownloadString(url);
            }
            return contentsRemote;
        }
    }
}
