using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Coypu;
using OpenQA.Selenium;

namespace AcceptanceTests
{
    public class ScreenShotDumper
    {
        public static void DumpScreenshotAndStackTrace(string name, Driver staticDriverReference)
        {
            SaveScreenshotTo(GetSaveLocation(name), staticDriverReference);
        }

        private static void SaveScreenshotTo(string imageFile, Driver staticDriverReference)
        {
            var screenshot = ((ITakesScreenshot)staticDriverReference.WebDriver).GetScreenshot();
            screenshot.SaveAsFile(imageFile, ImageFormat.Png);
        }

        private static string GetSaveLocation(string filename)
        {
            var buildConfName = Environment.GetEnvironmentVariable("TEAMCITY_BUILDCONF_NAME") ?? "buildConfName";
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "buildNumber";

            var projectFolder = GetProjectFolder();

            var screenshotFolder = CombinePaths(projectFolder, "logs", "buildScreenshots", buildConfName, buildNumber);
            Directory.CreateDirectory(screenshotFolder);

            return Path.Combine(screenshotFolder, EscapeFilename(filename) + ".png");
        }

        private static long SystemMillis()
        {
            var unixEpoch = new DateTime(1970, 1, 1);
            return (DateTime.Now.Ticks - unixEpoch.Ticks) / TimeSpan.TicksPerMillisecond;
        }

        private static string CombinePaths(string path1, params string[] paths)
        {
            if (path1 == null)
            {
                throw new ArgumentNullException("path1");
            }
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(path1, Path.Combine);
        }

        private static string GetProjectFolder()
        {
            var currentDirInfo = new DirectoryInfo(Environment.CurrentDirectory);

            while (currentDirInfo.Parent != null && !currentDirInfo.Name.Equals("source"))
            {
                currentDirInfo = currentDirInfo.Parent;
            }

            return currentDirInfo.Name.Equals("source")
                       ? currentDirInfo.Parent.FullName
                       : @"C:\Temp";
        }

        private static string EscapeFilename(string raw)
        {
            return Regex.Replace(raw, @"[\\\?:\+\*\s""<>|,]+", "_");
        }

    }
}