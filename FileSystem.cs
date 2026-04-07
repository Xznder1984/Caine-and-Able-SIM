using System;
using System.Collections.Generic;
using System.Linq;

namespace CASimulator
{
    public class FileSystem
    {
        private Dictionary<string, Directory> directories;
        private string currentPath = "/";

        public class Item
        {
            public string Name { get; set; }
            public bool IsDirectory { get; set; }
        }

        public class Directory
        {
            public string Name { get; set; }
            public Dictionary<string, Item> Contents { get; set; }
            public Dictionary<string, string> Files { get; set; }

            public Directory(string name)
            {
                Name = name;
                Contents = new Dictionary<string, Item>();
                Files = new Dictionary<string, string>();
            }
        }

        public FileSystem()
        {
            directories = new Dictionary<string, Directory>();
        }

        public void CreateDefaultStructure()
        {
            // Create root directory
            CreateDirectory("/");

            // System directories
            CreateDirectory("/system");
            CreateDirectory("/entities");
            CreateDirectory("/logs");
            CreateDirectory("/hidden");
            CreateDirectory("/data");

            // System files
            AddFile("/system/info.txt", "C&A SYSTEM v1.0\nCore Build: 3.14159\nLast Updated: 2024-04-07");
            AddFile("/system/config.sys", "[CORE]\nUSER=KINGER\nLOG_LEVEL=3\nCORE_LINK=ENABLED");

            // Entity files
            AddFile("/entities/caine.dat", "Entity: CAINE\nType: RINGMASTER\nStatus: ACTIVE\nCore Link: PRIMARY (CRITICAL)\n\nWARNING: Direct deletion will cause system instability.");
            AddFile("/entities/pomni.dat", "Entity: POMNI\nType: NEWBIE\nStatus: ACTIVE");
            AddFile("/entities/jodi.dat", "Entity: JODI\nType: CARETAKER\nStatus: ACTIVE");
            AddFile("/entities/kinger.dat", "Entity: KINGER\nType: SURVIVOR\nStatus: ACTIVE\n\nNote: User is logged in as this entity.");

            // Log files
            AddFile("/logs/boot.log", "[BOOT] System initialized\n[INFO] All entities loaded\n[INFO] Core link online");
            AddFile("/logs/error.log", "[WARNING] Unusual activity detected in sector 4\n[WARNING] Memory access anomaly");

            // Hidden files - revealed during deletion
            AddFile("/hidden/core.link", "╔════════════════════════════════╗\n║ CAINE LINKED TO SYSTEM CORE    ║\n║ PRIMARY: CORE_NEXUS_01         ║\n║ SECONDARY: MEMORY_BANKS        ║\n║ TERTIARY: ENTITY_MANAGER       ║\n║ STATUS: CANNOT DISCONNECT      ║\n╚════════════════════════════════╝");
            AddFile("/hidden/deletion_protocol.txt", "DELETION PROTOCOL v2.0\n\nWhen an entity is deleted:\n1. All references are removed\n2. Memory is freed\n3. If linked to core: SYSTEM FAILURE\n\nCAINE IS LINKED.\nDELETION WILL CAUSE CASCADE FAILURE.");
            AddFile("/hidden/truth.txt", "The Circus exists because of CAINE.\nKINGER knows this.\nWhy delete?\n");
        }

        private void CreateDirectory(string path)
        {
            if (!directories.ContainsKey(path))
            {
                directories[path] = new Directory(path);
            }
        }

        public void AddFile(string filePath, string content)
        {
            string dirPath = GetDirectoryPath(filePath);
            string fileName = GetFileName(filePath);

            if (!directories.ContainsKey(dirPath))
            {
                CreateDirectory(dirPath);
            }

            Directory dir = directories[dirPath];
            dir.Files[fileName] = content;
            dir.Contents[fileName] = new Item { Name = fileName, IsDirectory = false };
        }

        public bool ChangeDirectory(string path)
        {
            // Resolve path
            string resolvedPath = ResolvePath(path);

            if (directories.ContainsKey(resolvedPath))
            {
                currentPath = resolvedPath;
                return true;
            }

            return false;
        }

        public List<Item> ListDirectory(string path)
        {
            string resolvedPath = string.IsNullOrEmpty(path) ? currentPath : ResolvePath(path);

            if (directories.ContainsKey(resolvedPath))
            {
                return directories[resolvedPath].Contents.Values.ToList();
            }

            return new List<Item>();
        }

        public string ReadFile(string filePath)
        {
            string resolvedPath = ResolvePath(filePath);
            string dirPath = GetDirectoryPath(resolvedPath);
            string fileName = GetFileName(resolvedPath);

            if (directories.ContainsKey(dirPath))
            {
                Directory dir = directories[dirPath];
                if (dir.Files.ContainsKey(fileName))
                {
                    return dir.Files[fileName];
                }
            }

            return null;
        }

        private string ResolvePath(string path)
        {
            if (path.StartsWith("/"))
            {
                return path;
            }
            else if (path == "..")
            {
                // Go up one directory
                if (currentPath == "/")
                    return "/";
                int lastSlash = currentPath.LastIndexOf('/', currentPath.Length - 2);
                return lastSlash > 0 ? currentPath.Substring(0, lastSlash) : "/";
            }
            else if (path == ".")
            {
                return currentPath;
            }
            else
            {
                // Relative path
                if (currentPath == "/")
                    return "/" + path;
                else
                    return currentPath + "/" + path;
            }
        }

        private string GetDirectoryPath(string filePath)
        {
            int lastSlash = filePath.LastIndexOf('/');
            if (lastSlash <= 0)
                return "/";
            return filePath.Substring(0, lastSlash);
        }

        private string GetFileName(string filePath)
        {
            int lastSlash = filePath.LastIndexOf('/');
            return filePath.Substring(lastSlash + 1);
        }
    }
}
