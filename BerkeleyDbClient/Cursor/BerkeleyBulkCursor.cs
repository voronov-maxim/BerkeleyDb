using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public sealed class BerkeleyBulkCursor : BerkeleyBulkCursorBase
    {
        private MultipleBuffer _deleteBuffer;
        private readonly List<Byte[]> _deleteData;
        private MultipleBuffer _readBuffer;
        private MultipleBuffer _writeBuffer;
        private readonly List<Dto.BerkeleyDtoPut> _writeData;

        public BerkeleyBulkCursor(BerkeleyDb berkeleyDb, int bufferSize) :
            base(berkeleyDb, bufferSize, BerkeleyDbMultiple.DB_MULTIPLE_KEY)
        {
            _deleteBuffer = new MultipleBuffer(bufferSize);
            _deleteData = new List<Byte[]>();
            _writeBuffer = new MultipleBuffer(bufferSize);
            _writeData = new List<Dto.BerkeleyDtoPut>();
        }

        public void AddDelete(Byte[] key)
        {
            if (_deleteBuffer.AddRecord(key))
                return;

            if (_deleteBuffer.Position == 0)
                _deleteData.Add(key);
            else
            {
                CloseDeleteBuffer();
                AddDelete(key);
            }
        }
        public void AddWrite(Byte[] key, Byte[] value)
        {
            if (_writeBuffer.AddRecord(key))
            {
                if (_writeBuffer.AddRecord(value))
                    return;

                _writeBuffer.RemoveRecord(key.Length);
            }

            if (_writeBuffer.Position == 0)
                _writeData.Add(new Dto.BerkeleyDtoPut(key, value));
            else
            {
                CloseWriteBuffer();
                AddWrite(key, value);
            }
        }
        private void CloseDeleteBuffer()
        {
            if (_deleteBuffer.Position > 0)
            {
                Byte[] buffer = _deleteBuffer.Close();
                _deleteData.Add(buffer);
            }
        }
        private void CloseWriteBuffer()
        {
            if (_writeBuffer.Position > 0)
            {
                Byte[] buffer = _writeBuffer.Close();
                _writeData.Add(new Dto.BerkeleyDtoPut(null, buffer));
            }
        }
        public async Task<BerkeleyError> DeleteAsync()
        {
            CloseDeleteBuffer();

            foreach (Byte[] data in _deleteData)
            {
                BerkeleyDbMultiple multiple = data.Length > base.BufferSize ? 0 : BerkeleyDbMultiple.DB_MULTIPLE;
                BerkeleyError error = await base.BerkeleyDb.DeleteAsync(data, 0, multiple).ConfigureAwait(false);
                if (error.HasError)
                    return error;
            }

            _deleteData.Clear();
            return BerkeleyError.NoError;
        }
        internal override bool GetNext(out BerkeleyKeyValueBulk keyValue)
        {
            ArraySegment<Byte> key, value;
            if (_readBuffer.GetNextRecord(out key) && _readBuffer.GetNextRecord(out value))
            {
                keyValue = new BerkeleyKeyValueBulk(key, value);
                return true;
            }
            else
            {
                keyValue = new BerkeleyKeyValueBulk();
                return false;
            }
        }
        protected override void SetDtoGet(ref Dto.BerkeleyDtoGet dataGet)
        {
            _readBuffer = new MultipleBuffer(dataGet.Value);
        }
        public async Task<BerkeleyError> WriteAsync(BerkeleyDbWriteMode writeMode)
        {
            CloseWriteBuffer();

            foreach (Dto.BerkeleyDtoPut data in _writeData)
            {
                BerkeleyError error = await base.BerkeleyDb.Methods.WriteMultipleAsync(base.BerkeleyDb, data, writeMode).ConfigureAwait(false);
                if (error.HasError)
                    return error;
            }

            _writeData.Clear();
            return BerkeleyError.NoError;
        }
    }
}
