using Microsoft.Owin.Hosting;
using System;
using System.IO.Pipes;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace BerkeleyDbWebApiSelfHost
{
    class Program
    {
        static void Main(String[] args)
        {
            if (args.Length == 1 && args[0].ToLower() == "close")
            {
                PipeClient();
                return;
            }

            var pipeServerThread = new System.Threading.Thread(PipeServer);
            pipeServerThread.IsBackground = true;
            pipeServerThread.Start();

            using (WebApp.Start<Startup>("http://localhost:9001/"))
            {
                Console.WriteLine("Press enter to close berkeley db web api host");
                Console.ReadLine();
            }
        }

        private static void PipeServer()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("BerkeleyDbWebApiSelfHost", PipeDirection.In))
                pipeServer.WaitForConnection();

            IntPtr stdin = GetStdHandle(StdHandle.Stdin);
            CloseHandle(stdin);
        }
        private static void PipeClient()
        {
            using (var pipeClient = new NamedPipeClientStream(".", "BerkeleyDbWebApiSelfHost", PipeDirection.Out))
                pipeClient.Connect();
        }

        private enum StdHandle
        {
            Stdin = -10, Stdout = -11, Stderr = -12
        };

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hdl);

    }
}
