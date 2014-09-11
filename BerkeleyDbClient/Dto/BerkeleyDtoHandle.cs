using System;

namespace BerkeleyDbClient.Dto
{
    public struct BerkeleyDtoResult
    {
        public BerkeleyDbError Error;
        public String Result;

        public BerkeleyDtoResult(BerkeleyDbError error, String result)
        {
            Error = error;
            Result = result;
        }
    }
}
