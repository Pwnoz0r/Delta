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
using System.Text;
using System.Threading.Tasks;
using DeltaPatching.Util;
using System.Net;

namespace DeltaPatching
{
    public class Hash
    {
        /// <summary>
        /// Generate a hash from the given file path.
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <param name="outputFile">Create a (filename).md5 file when generating the hash.</param>
        /// <returns>Filename and hash in the following format: "filename:hash"</returns>
        public static string GenerateHash(string file, bool outputFile = false)
        {
            string computedHash = "";

            using (var md5 = MD5.Create())
                using (var fileStream = File.OpenRead(file))
                    computedHash = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLower();

            Console.WriteLine(string.Format("{0}:{1}", Path.GetFileName(file), computedHash));

            if (outputFile)
                using (StreamWriter sw = new StreamWriter(Path.Combine(Config.DirectoryOutput, string.Format("{0}.md5", Path.GetFileName(file)))))
                    sw.Write(computedHash);

            return computedHash;
        }

        /// <summary>
        /// Generate hash of a given directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="fileFilter">File filter. * is an all file wildcard (default): fileFilter = "*.dll". Multiple wildcards can be used: fileFilter = "*.exe,*.dll"</param>
        /// <returns>Compiled list of all file names and their hashes.</returns>
        public static List<string> GenerateHashFilesInDirectory(string path, string outputFolder, string fileFilter = "*")
        {
            string[] files = Directory.GetFiles(path);
            List<string> compiledList = new List<string>();

            if (!Directory.Exists(Config.DirectoryOutput))
                Directory.CreateDirectory(Config.DirectoryOutput);

            foreach (string fileName in files)
            {
                if (fileFilter == "*")
                    compiledList.Add(string.Format("{0}:{1}", Path.GetFileName(fileName), GenerateHash(fileName)));
                else
                    foreach (string filter in fileFilter.Split(','))
                        if (Path.GetExtension(fileName).Contains(filter.TrimStart('*')))
                            compiledList.Add(string.Format("{0}:{1}", Path.GetFileName(fileName), GenerateHash(fileName)));
            }

            string hashPath = Path.GetDirectoryName(path);
            int index = hashPath.LastIndexOf(@"\");
            if (index != -1)
                hashPath = hashPath.Substring(index).TrimStart('\\');

            string hashListPath = Path.Combine(outputFolder, string.Format("{0}.txt", hashPath));

            if (File.Exists(hashListPath))
                File.Delete(hashListPath);

            foreach (string s in compiledList)
                using (StreamWriter sw = new StreamWriter(hashListPath, true))
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
            List<string> d1 = new List<string>();
            List<string> d2 = new List<string>();
            List<string> remoteDifferences = new List<string>();
            List<string> clientDifferences = new List<string>();
            Dictionary<string, string> allDiff = new Dictionary<string, string>();

            using (StringReader r = new StringReader(GetLocalHash(localHashFile)))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    d1.Add(line);
            }

            using (StringReader r = new StringReader(GetRemoteHash(remoteHashFile)))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    d2.Add(line);
            }

            remoteDifferences = d2.Except(d1).ToList();
            clientDifferences = d1.Except(d2).ToList();

            // R = REMOTE : C = CLIENT
            foreach (string s in remoteDifferences)
                allDiff.Add(s, "R");
            foreach (string s in clientDifferences)
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
            string contentsRemote = "";
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                contentsRemote = wc.DownloadString(url);
            }
            return contentsRemote;
        }
    }
}
