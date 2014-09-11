using System;
using System.Diagnostics;
using System.IO;

namespace BerkeleyDbWebApiUnitTest
{
    public static class OwinSelfHostHelper
    {
        public static void Run()
        {
            Process.Start(GetHostFileName());
        }
        public static void Stop()
        {
            Process.Start(GetHostFileName(), "close");
        }
        private static String GetHostFileName()
        {
            String fileName = typeof(OwinSelfHostHelper).Assembly.Location;
            fileName = fileName.Replace("BerkeleyDbWebApiUnitTest", "BerkeleyDbWebApiSelfHost");
            return Path.ChangeExtension(fileName, "exe");
        }
    }
}
