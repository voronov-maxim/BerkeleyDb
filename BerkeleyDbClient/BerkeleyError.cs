using BerkeleyDbClient;
using System;
using System.Net;

namespace BerkeleyDbClient
{
    public struct BerkeleyError
    {
        public static readonly BerkeleyError NoError = new BerkeleyError(HttpStatusCode.OK, 0);

        private readonly BerkeleyDbError _berkeleyDbError;
        private readonly HttpStatusCode _httStatusCode;

        public BerkeleyError(HttpStatusCode httStatusCode)
            : this(httStatusCode, 0)
        {

        }
        public BerkeleyError(BerkeleyDbError berkeleyDbError)
            : this(HttpStatusCode.OK, berkeleyDbError)
        {

        }
        public BerkeleyError(HttpStatusCode httStatusCode, BerkeleyDbError berkeleyDbError)
        {
            _httStatusCode = httStatusCode;
            _berkeleyDbError = berkeleyDbError;
        }

        public static bool IsSuccessStatusCode(HttpStatusCode httpStatusCode)
        {
            return httpStatusCode >= HttpStatusCode.OK && httpStatusCode <= (HttpStatusCode)299; 
        }
        public void ThrowIfError()
        {
            if (HasError)
                throw new BerkeleyException(_httStatusCode, _berkeleyDbError);
        }
        public static void ThrowIfError(HttpStatusCode httpStatusCode)
        {
            if (!IsSuccessStatusCode(httpStatusCode))
                throw new BerkeleyException(httpStatusCode, 0);
        }
        public override String ToString()
        {
            if (HasError)
                return new BerkeleyException(_httStatusCode, _berkeleyDbError).ToString();
            else
                return "success";
        }

        public BerkeleyDbError BerkeleyDbError
        {
            get
            {
                return _berkeleyDbError;
            }
        }
        public bool HasError
        {
            get
            {
                return !(_berkeleyDbError == 0 && _httStatusCode >= HttpStatusCode.OK && _httStatusCode <= (HttpStatusCode)299);
            }
        }
        public HttpStatusCode HttStatusCode
        {
            get
            {
                return _httStatusCode;
            }
        }
    }

    public sealed class BerkeleyException : Exception
    {
        private readonly BerkeleyDbError _berkeleyDbError;
        private readonly HttpStatusCode _httpStatusCode;

        public BerkeleyException(HttpStatusCode httpStatusCode, BerkeleyDbError berkeleyDbError)
        {
            _httpStatusCode = httpStatusCode;
            _berkeleyDbError = berkeleyDbError;
        }

        public override String ToString()
        {
            if (BerkeleyDbError == 0)
                return "HttpStatusCode = " + HttpStatusCode.ToString();
            else
                return "BerkeletDbError = " + BerkeleyDbError.ToString();
        }
        public BerkeleyDbError BerkeleyDbError
        {
            get
            {
                return _berkeleyDbError;
            }
        }
        public HttpStatusCode HttpStatusCode
        {
            get
            {
                return _httpStatusCode;
            }
        }
    }

    public struct BerkeleyResult<T>
    {
        private readonly BerkeleyError _error;
        private readonly T _result;

        public BerkeleyResult(T result)
        {
            _result = result;
            _error = BerkeleyError.NoError;
        }
        public BerkeleyResult(HttpStatusCode httpStatusCode)
        {
            _error = new BerkeleyError(httpStatusCode);
            _result = default(T);
        }
        public BerkeleyResult(BerkeleyDbError berkeleyDbError)
        {
            _error = new BerkeleyError(berkeleyDbError);
            _result = default(T);
        }
        public BerkeleyResult(BerkeleyDbError berkeleyDbError, T result)
        {
            _error = new BerkeleyError(berkeleyDbError);
            _result = result;
        }
        public BerkeleyResult(BerkeleyError error)
        {
            _error = error;
            _result = default(T);
        }

        public BerkeleyError Error
        {
            get
            {
                return _error;
            }
        }
        public bool HasError
        {
            get
            {
                return _error.HasError;
            }
        }
        public T Result
        {
            get
            {
                return _result;
            }
        }
    }
}
