using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LF_to_CRLF
{
    internal class Program
    {
        const string temp = @"C:\temp_LF_to_CRLF\";

        readonly static string[] fileExtentions = new[] { "cs", "resx", "css", "scss", "cshtml", "svg", "json", "js" };

        const byte CR = 13;
        const byte LF = 10;

        static void Main(string[] args)
        {
            Directory.CreateDirectory(temp);

            var fileNames = FileAllFiles(@"put-path-to-yout-directory-here");

            var tasks = fileNames.Select(i => FixFileAsync(i)).ToArray();

            while (tasks.Any())
            {
                Task.WaitAny(tasks);

                tasks = tasks.Where(i => !i.IsCompleted).ToArray();

                Console.WriteLine(100 - 100 * tasks.Length / fileNames.Count + "%");
            }

            Console.WriteLine("Done!");
        }

        private static IReadOnlyCollection<string> FileAllFiles(string pathToDirectory)
        {
            var files = new List<string>();

            foreach (var fileExtension in fileExtentions)
            {
                var searchPattern = "*." + fileExtension;
                files.AddRange(Directory.GetFiles(pathToDirectory, searchPattern, SearchOption.AllDirectories));
            }

            return files;
        }

        static async Task FixFileAsync(string fileName)
        {
            var bytes = await File.ReadAllBytesAsync(fileName);

            var tempFileName = temp + Guid.NewGuid();

            byte previousByte = 0;

            using (var tempFile = File.Create(tempFileName))
            {
                foreach (var oneByte in bytes)
                {
                    if (oneByte == LF && previousByte != CR)
                    {
                        tempFile.WriteByte(CR);
                    }

                    tempFile.WriteByte(oneByte);

                    previousByte = oneByte;
                }
            }

            File.Replace(tempFileName, fileName, null);

            File.Delete(tempFileName);
        }
    }
}
