using System;

namespace BerkeleyDbClient
{
    public static class BerkeleyEnumParser
    {
        public static BerkeleyDbCursorFlags CursorFlags(String flags)
        {
            BerkeleyDbCursorFlags value = 0;
            foreach (String flag in flags.Split(','))
                value |= (BerkeleyDbCursorFlags)Enum.Parse(typeof(BerkeleyDbCursorFlags), flags, true);
            return value;
        }
        public static BerkeleyDbEnvClose EnvCloseFlags(String flags)
        {
            switch (flags.ToLower())
            { 
                case "0":
                    return 0;
                case "db_forcesync":
                    return BerkeleyDbEnvClose.DB_FORCESYNC;
                case "db_forcesyncenv":
                    return BerkeleyDbEnvClose.DB_FORCESYNCENV;
                default:
                    throw new ArgumentOutOfRangeException("flags", flags);
            }
        }
        public static BerkeleyDbEnvOpen EnvOpenFlags(String flags)
        {
            BerkeleyDbEnvOpen value = 0;
            foreach (String flag in flags.Split(','))
                value |= (BerkeleyDbEnvOpen)Enum.Parse(typeof(BerkeleyDbEnvOpen), flags, true);
            return value;
        }
        public static BerkeleyDbFlags Flags(String flags)
        {
            BerkeleyDbFlags value = 0;
            foreach (String flag in flags.Split(','))
                value |= (BerkeleyDbFlags)Enum.Parse(typeof(BerkeleyDbFlags), flags, true);
            return value;
        }
        public static BerkeleyDbOpenFlags OpenFlags(String flags)
        {
            BerkeleyDbOpenFlags value = 0;
            foreach (String flag in flags.Split(','))
                value |= (BerkeleyDbOpenFlags)Enum.Parse(typeof(BerkeleyDbOpenFlags), flags, true);
            return value;
        }
    }
}
