using System;
using System.Collections.Generic;
using System.IO;

namespace MediaHelper
{
    class Program
    {
        public enum SupportedExtensions { MP4, MKV, AVI, SRT }
        public enum ExtensionsToDelete { JPG, TXT, TORRENT, DAT, NFO }
        static void Main(string[] args)
        {
            try
            {
                List<string> directories = new List<string>() { @"D:\Media\Movies", @"M:\Movies" };
                foreach (var d in directories)
                {
                    if (Directory.Exists(d))
                    {
                        ProcessDirectory(d);
                    }
                }

                string downloadsDir = @"C:\Users\bertr\Downloads";
                ProcessFiles(downloadsDir, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                throw;
            }

        }

        public static void ProcessDirectory(string targetDirectory)
        {
            try
            {
                // Process the list of files found in the directory.
                ProcessFiles(targetDirectory);

                // Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    try
                    {
                        string newName = subdirectory;
                        if (newName.Contains("["))
                        {
                            newName = newName.Substring(0, newName.IndexOf("[")).TrimEnd();
                            Directory.Move(subdirectory, newName);
                            ProcessFiles(newName);
                        }

                        ProcessDirectory(newName);
                        if (Directory.GetFiles(newName).Length == 0 &&
                            Directory.GetDirectories(newName).Length == 0)
                        {
                            Directory.Delete(subdirectory, false);
                        }
                    }
                    catch (Exception ex1)
                    {
                        throw new Exception($"{subdirectory} {ex1.Message}");
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{targetDirectory} {ex.Message}", ex);
            }
        }

        private static void ProcessFiles(string targetDirectory, bool onlyTorrents = false)
        {

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                try
                {
                    string name = Path.GetFileName(targetDirectory);
                    string extension = Path.GetExtension(fileName).Replace(".", string.Empty);

                    if (Enum.TryParse(extension.ToUpper(), out SupportedExtensions ext) && !onlyTorrents)
                    {
                        string extensionPrefix = (ext == SupportedExtensions.SRT) ? "en." : string.Empty;
                        RenameFile(targetDirectory, fileName, name, $"{extensionPrefix}{extension}");
                    }
                    else if (Enum.TryParse(extension.ToUpper(), out ExtensionsToDelete ext2))
                    {
                        if (onlyTorrents)
                        {
                            if (ext2 == ExtensionsToDelete.TORRENT)
                            {
                                DeleteFile(fileName);
                            }
                        }
                        else
                        {
                            DeleteFile(fileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"{fileName} {ex.Message}", ex);
                }
            }
        }

        private static void DeleteFile(string fileName)
        {
            File.SetAttributes(fileName, FileAttributes.Normal);
            File.Delete(fileName);
        }

        public static void RenameFile(string directoryName, string oldName, string newName, string extension)
        {
            try
            {
                File.Move(oldName, $@"{directoryName}\{newName}.{extension}");
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                throw;
            }
        }
    }
}