using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace beatoraja_installer
{
    class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return InternalMainAsync(args).Result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Task.Delay(2000).Wait();
                return Marshal.GetHRForException(ex);
            }
        }

        private static async Task<int> InternalMainAsync(string[] args)
        {
            if (!System.Environment.Is64BitProcess)
            {
                Console.Error.WriteLine("This is not 64bit OS.");
                Task.Delay(1000).Wait();
                return 1;
            }
            else
            {
                Console.Out.WriteLine("This is 64bit OS.");
            }

            await InstallJavaAsync();

            await InstallBeatoraja();

            Console.Out.WriteLine("Install LITONE5?");
            Console.Out.WriteLine("(FHD Recommended Requirements : 8GB RAM, 4GB VRAM)");
            Console.Out.WriteLine("[Yes(Y)/No(N)]");
            string c = Console.ReadLine();

            if (c == "Y" || c == "Yes")
            {
                InstallLITONE5();
            }

            Console.Out.WriteLine("Complete!!");
            Task.Delay(2000).Wait();

            return 0;
        }

        public static async Task<byte[]> DownloadFile(string url)
        {
            using (var client = new HttpClient())
            {

                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }

                }
            }
            return null;
        }

        private static async Task InstallJavaAsync()
        {
            int result = 0;
            using (var process = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/c \"java -version\""
                }
            })
            {
                process.Start();
                process.WaitForExit();
                result = process.ExitCode;
            }

            if (result != 0)
            {
                // install
                Console.Out.WriteLine("Downloading Java installer...");
                var path = @"jre-8u181-windows-x64.exe";
                var fileByte = await DownloadFile("http://javadl.oracle.com/webapps/download/AutoDL?BundleId=234474_96a7b8442fe848ef90c96a2fad6ed6d1");
                using (var saveStream = new MemoryStream(fileByte) { Position = 0 })
                {

                    using (var fileStream = System.IO.File.Create(path))
                    {
                        saveStream.CopyTo(fileStream);
                    }
                }

                using (var installerProcess = new Process() { StartInfo = new ProcessStartInfo(path) })
                {
                    installerProcess.Start();
                    installerProcess.WaitForExit();
                }
            }
            else
            {
                Console.Out.WriteLine("Java is already installed.");
            }

            return;
        }

        private static async Task InstallBeatoraja()
        {
            Console.Out.WriteLine("Downloading beatoraja...");
            var path = @"beatoraja.zip";
            var fileByte = await DownloadFile("https://mocha-repository.info/download/beatoraja0.6.1.zip");

            Console.Out.WriteLine("Unzip beatoraja...");
            using (var saveStream = new MemoryStream(fileByte) { Position = 0 })
            {
                using (var fileStream = System.IO.File.Create(path))
                {
                    saveStream.CopyTo(fileStream);
                }
            }

            ZipFile.ExtractToDirectory(path, @"./");
        }

        private static void InstallLITONE5()
        {
            Console.Out.WriteLine("Downloading LITONE5...");
            FileDownloader.DownloadFileFromURLToPath($"https://drive.google.com/uc?id=15JN3UUS2up8U80X-79nLFukVZamBwKjB", @"litone5.rar");

            Console.Out.WriteLine("Unzip LITONE5...");
            using (var archive = RarArchive.Open(@"litone5.rar"))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(@"beatoraja0.6.1/skin", new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }
}
