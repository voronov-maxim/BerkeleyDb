using System;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public abstract class BerkeleyDbMethodsAsync
    {
        public abstract BerkeleyCursorMethodsAsync CreateCursorMethods();

        public abstract Task<BerkeleyError> CloseDbAsync(BerkeleyDb db, BerkeleyDbClose flags);
        public abstract Task<BerkeleyResult<long>> CreateDb(BerkeleyDb berkeleyDb, BerkeleyDbFlags flags);
        public abstract Task<BerkeleyError> DeleteAsync(BerkeleyDb db, Byte[] key, BerkeleyDbDelete flag, BerkeleyDbMultiple multiple);
        public abstract Task<BerkeleyResult<int>> GetPageSizeAsync(BerkeleyDb db);
        public abstract Task<BerkeleyError> OpenDbAsync(BerkeleyDb db, String name, BerkeleyDbOpenFlags flags);
        public abstract Task<BerkeleyResult<long>> OpenCursorAsync(BerkeleyDb db, BerkeleyDbCursorFlags flags);
        public abstract Task<BerkeleyError> WriteAsync(BerkeleyDb db, Byte[] key, Byte[] value, BerkeleyDbWriteMode writeMode);
        public abstract Task<BerkeleyError> WriteDuplicateAsync(BerkeleyDb db, Dto.BerkeleyDtoPut data, int bufferSize, BerkeleyDbWriteMode writeMode);
        public abstract Task<BerkeleyError> WriteMultipleAsync(BerkeleyDb db, Dto.BerkeleyDtoPut data, BerkeleyDbWriteMode writeMode);
    }
}
