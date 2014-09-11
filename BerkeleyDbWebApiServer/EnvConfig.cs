using BerkeleyDbClient;
using System;
using System.Linq;
using System.Xml.Linq;

namespace BerkeleyDbWebApiServer
{
    public sealed class EnvConfig
    {
        private readonly BerkeleyDbEnvClose _closeFlags;
        private readonly String _dbHome;
        private readonly BerkeleyDbEnvOpen _openFlags;
        private readonly bool _useTempIfFault;

        public EnvConfig(String dbHome, bool useTempIfFault, BerkeleyDbEnvOpen openFlags, BerkeleyDbEnvClose closeFlags)
        {
            _dbHome = dbHome;
            _useTempIfFault = useTempIfFault;
            _openFlags = openFlags;
            _closeFlags = closeFlags;
        }

        public BerkeleyDbEnvClose CloseFlags
        {
            get
            {
                return _closeFlags;
            }
        }
        public String DbHome
        {
            get
            {
                return _dbHome;
            }
        }
        public BerkeleyDbEnvOpen OpenFlags
        {
            get
            {
                return _openFlags;
            }
        }
        public bool UseTempIfFault
        {
            get
            {
                return _useTempIfFault;
            }
        }
    }

    public static class EnvConfigReader
    {
        public static EnvConfig ReadXmlFile()
        {
            var root = XElement.Load("envconfig.xml");
            XElement xopen = root.Elements("open").Single();

            XElement xdbhome = xopen.Elements("dbhome").Single();
            bool useTempIfFault;
            Boolean.TryParse(xdbhome.Attribute("useTempIfFault").Value, out useTempIfFault);

            BerkeleyDbEnvOpen openFlags = BerkeleyEnumParser.EnvOpenFlags(xopen.Element("flags").Value.Trim());

            XElement xclose = root.Elements("close").Single();
            BerkeleyDbEnvClose closeFlags = BerkeleyEnumParser.EnvCloseFlags(xclose.Element("flags").Value.Trim());

            return new EnvConfig(xdbhome.Value.Trim(), useTempIfFault, openFlags, closeFlags);
        }

    }
}
