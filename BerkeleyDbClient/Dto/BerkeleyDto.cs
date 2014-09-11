using System;

namespace BerkeleyDbClient.Dto
{
    public struct BerkeleyDtoGet
    {
        public int ErrorCode;
        public Byte[] Key;
        public Byte[] Value;
    }

    public struct BerkeleyDtoPartial
    {
        public Byte[] Data;
        public int Length;
        public int Offset;
    }

    public struct BerkeleyDtoPartialPut
    {
        public Byte[] Key;
        public BerkeleyDtoPartial Value;
    }

    public struct BerkeleyDtoPut
    {
        public Byte[] Key;
        public Byte[] Value;

        public BerkeleyDtoPut(Byte[] key, Byte[] value)
        {
            Key = key;
            Value = value;
        }
    }
}
