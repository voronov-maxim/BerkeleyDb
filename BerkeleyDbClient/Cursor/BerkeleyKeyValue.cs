using System;

namespace BerkeleyDbClient
{
    public struct BerkeleyKeyValue
    {
        private readonly Byte[] _key;
        private readonly Byte[] _value;

        public BerkeleyKeyValue(Byte[] key, Byte[] value)
        {
            _key = key;
            _value = value;
        }

        public Byte[] Key
        {
            get
            {
                return _key;
            }
        }
        public Byte[] Value
        {
            get
            {
                return _value;
            }
        }
    }

    public struct BerkeleyKeyValueBulk
    {
        private readonly ArraySegment<Byte> _key;
        private readonly ArraySegment<Byte> _value;

        public BerkeleyKeyValueBulk(Byte[] key, ArraySegment<Byte> value)
        {
            _key = new ArraySegment<Byte>(key);
            _value = value;
        }
        public BerkeleyKeyValueBulk(ArraySegment<Byte> key, ArraySegment<Byte> value)
        {
            _key = key;
            _value = value;
        }

        public ArraySegment<Byte> Key
        {
            get
            {
                return _key;
            }
        }
        public ArraySegment<Byte> Value
        {
            get
            {
                return _value;
            }
        }
    }
}
