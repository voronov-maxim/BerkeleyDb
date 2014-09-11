using System;
using System.Collections;
using System.Collections.Generic;

namespace BerkeleyDbClient
{
    public struct BerkeleyBulkEnumerator : IEnumerable<BerkeleyKeyValueBulk>, IEnumerator<BerkeleyKeyValueBulk>
    {
        private BerkeleyKeyValueBulk _current;
        private readonly BerkeleyBulkCursorBase _cursor;
        private readonly BerkeleyError _error;

        public BerkeleyBulkEnumerator(BerkeleyBulkCursorBase cursor)
        {
            _cursor = cursor;

            _current = new BerkeleyKeyValueBulk();
            _error = BerkeleyError.NoError;
        }
        public BerkeleyBulkEnumerator(BerkeleyError error)
        {
            _error = error;

            _cursor = null;
            _current = new BerkeleyKeyValueBulk();
        }

        public void Dispose()
        {
        }
        public BerkeleyBulkEnumerator GetEnumerator()
        {
            return this;
        }
        public bool MoveNext()
        {
            if (_cursor == null)
                throw new InvalidOperationException("cannot iterate errored cursor");

            return _cursor.GetNext(out _current);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        IEnumerator<BerkeleyKeyValueBulk> IEnumerable<BerkeleyKeyValueBulk>.GetEnumerator()
        {
            return this;
        }
        void IEnumerator.Reset()
        {
            throw new NotSupportedException("BerkeleyBulkEnumerator.Reset");
        }


        public BerkeleyKeyValueBulk Current
        {
            get
            {
                return _current;
            }
        }
        public BerkeleyError Error
        {
            get
            {
                return _error;
            }
        }
        public bool NotFound
        {
            get
            {
                return Error.BerkeleyDbError == BerkeleyDbError.DB_NOTFOUND;
            }
        }

        Object IEnumerator.Current
        {
            get
            {
                return _current;
            }
        }
    }
}
