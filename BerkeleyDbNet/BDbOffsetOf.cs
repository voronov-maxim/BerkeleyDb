using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace BerkeleyDbNet
{
    public sealed class BDbOffsetOf
    {
        private readonly BDbOffsetOfCollection _db;
        private readonly BDbOffsetOfCollection _dbc;
        private readonly BDbOffsetOfCollection _dbenv;

        public BDbOffsetOf(BDbOffsetOfCollection db, BDbOffsetOfCollection dbc, BDbOffsetOfCollection dbenv)
        {
            _db = db;
            _dbc = dbc;
            _dbenv = dbenv;
        }

        public BDbOffsetOfCollection Db
        {
            get
            {
                return _db;
            }
        }
        public BDbOffsetOfCollection Dbc
        {
            get
            {
                return _dbc;
            }
        }
        public BDbOffsetOfCollection Dbenv
        {
            get
            {
                return _dbenv;
            }
        }
    }

    public sealed class BDbOffsetOfCollection : ReadOnlyCollection<BDbOffsetOfItem>
    {
        public BDbOffsetOfCollection(IEnumerable<BDbOffsetOfItem> offsetOfs)
            : base(offsetOfs.ToArray())
        {
        }

        public BDbOffsetOfItem this[String name]
        {
            get
            {
                var offsetOfItems = base.Items as BDbOffsetOfItem[];
                for (int i = 0; i < offsetOfItems.Length; i++)
                    if (String.CompareOrdinal(offsetOfItems[i].Name, name) == 0)
                        return offsetOfItems[i];

                throw new ArgumentOutOfRangeException("name", name);
            }
        }
    }

    public static class BDbOffsetOfInstance
    {
        private static BDbOffsetOf _instance;

        public static BDbOffsetOf SetInstance(BDbOffsetOf offsetOf)
        {
            if (_instance != null)
                throw new InvalidOperationException("OffsetOf already created");

            lock (typeof(BDbOffsetOf))
            {
                if (_instance != null)
                    throw new InvalidOperationException("OffsetOf already created");

                _instance = offsetOf;
            }

            return _instance;
        }

        public static BDbOffsetOf Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("must call OffsetOf.SetInstance");

                return _instance;
            }
        }
    }

    public struct BDbOffsetOfItem
    {
        public readonly String Name;
        public readonly int Offset;

        public BDbOffsetOfItem(String name, int offset)
        {
            Name = name;
            Offset = offset;
        }
    }

    public static class BDbOffsetOfReader
    {
        private static BDbOffsetOfCollection GetOffsetOfList(XElement xversion, String typeName)
        {
            IEnumerable<BDbOffsetOfItem> offsetOfs = xversion.Elements(typeName).Single().Elements().
                Select(e => new BDbOffsetOfItem(e.Name.LocalName, Int32.Parse(e.Attribute("of").Value)));
            return new BDbOffsetOfCollection(offsetOfs);
        }
        public static BDbOffsetOf ReadXmlFile()
        {
            var root = XElement.Load("offsetof.xml");
            String version = root.Elements("current").Single().Attribute("version").Value;
            XElement xversion = root.Elements(version).Single();

            BDbOffsetOfCollection db = GetOffsetOfList(xversion, "db");
            BDbOffsetOfCollection dbc = GetOffsetOfList(xversion, "dbc");
            BDbOffsetOfCollection dbenv = GetOffsetOfList(xversion, "dbenv");
            return new BDbOffsetOf(db, dbc, dbenv);
        }
    }
}
