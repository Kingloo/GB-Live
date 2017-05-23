using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace GBLive.Tests
{
    public static class Utilities
    {
        public static string LoadTextFromFile(string filename)
        {
            string directory = TestContext.CurrentContext.TestDirectory;

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, "directory not found: {0}", directory));
            }

            string path = Path.Combine(directory, filename);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("directory exists, but file does not", filename);
            }

            return File.ReadAllText(path);
        }
    }
}
