using System;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public abstract class BerkeleyCursor : IDisposable
    {
        private const long InvalidHandle = 0;

        private readonly BerkeleyDb _berkeleyDb;
        private int _bufferSize;
        private BerkeleyDbCursorFlags _cursorFlags;
        private long _handle;
        private readonly BerkeleyDbCursorFlags _mandatoryFlags;
        private readonly BerkeleyCursorMethodsAsync _methods;

        protected BerkeleyCursor(BerkeleyDb berkeleyDb, int bufferSize, BerkeleyDbCursorFlags mandatoryFlags)
        {
            _berkeleyDb = berkeleyDb;
            _bufferSize = bufferSize;
            _mandatoryFlags = mandatoryFlags;

            _handle = InvalidHandle;
            _methods = berkeleyDb.Methods.CreateCursorMethods();
        }

        public async Task<BerkeleyError> CloseAsync()
        {
            if (_handle == InvalidHandle)
                return BerkeleyError.NoError;

            BerkeleyError error = await _methods.CloseCursorAsync(this).ConfigureAwait(false);
            _handle = InvalidHandle;
            return error;
        }
        public void Dispose()
        {
            CloseAsync().Wait();
        }
        public async Task<BerkeleyError> DeleteCurrentAsync(BerkeleyDbDelete flag = 0)
        {
            BerkeleyError error = await OpenAsync().ConfigureAwait(false);
            if (error.HasError)
                return error;

            return await Methods.DeleteCurrentAsync(this, flag).ConfigureAwait(false);
        }
        protected async Task<BerkeleyError> OpenAsync()
        {
            if (_handle == InvalidHandle)
            {
                BerkeleyResult<long> result = await BerkeleyDb.Methods.OpenCursorAsync(BerkeleyDb, CursorFlags).ConfigureAwait(false);
                if (result.HasError)
                    return result.Error;

                _handle = result.Result;
            }
            return BerkeleyError.NoError;
        }

        public BerkeleyDb BerkeleyDb
        {
            get
            {
                return _berkeleyDb;
            }
        }
        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
        }
        public BerkeleyDbCursorFlags CursorFlags
        {
            get
            {
                return _cursorFlags;
            }
            set
            {
                _cursorFlags = value | _mandatoryFlags;
            }
        }
        public long Handle
        {
            get
            {
                return _handle;
            }
        }
        protected BerkeleyCursorMethodsAsync Methods
        {
            get
            {
                return _methods;
            }
        }
    }
}
