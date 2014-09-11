using BerkeleyDbClient;
using BerkeleyDbClient.Dto;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace BerkeleyDbWebApiClient
{
    public sealed class BerkeleyDbWebApiMethods : BerkeleyDbMethodsAsync
    {
        private readonly HttpClient _httpClient;
        private readonly JsonMediaTypeFormatter _formatter;
        private readonly JsonSerializer _serializer;

        public BerkeleyDbWebApiMethods(HttpClient httpClient, JsonSerializer serializer, JsonMediaTypeFormatter formatter)
        {
            _httpClient = httpClient;
            _serializer = serializer;
            _formatter = formatter;
        }

        public override BerkeleyCursorMethodsAsync CreateCursorMethods()
        {
            return new BerkeleyCursorWebApiMethods(this, new JsonMediaTypeFormatter());
        }

        public override async Task<BerkeleyError> CloseDbAsync(BerkeleyDb db, BerkeleyDbClose flags)
        {
            String requestUri = "api/database/close/?handle=" + db.Handle.ToString() + "&flags=" + flags.ToStringEx();
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyResult<long>> CreateDb(BerkeleyDb berkeleyDb, BerkeleyDbFlags flags)
        {
            String requestUri = "api/database/create/?type=" + berkeleyDb.DbType.ToStringEx() + "&flags=" + flags.ToStringEx();
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyResult<long>(response.StatusCode);

            BerkeleyDtoResult dtoHandle = await SerializeHelper.GetResultAsync(_serializer, response.Content).ConfigureAwait(false);
            if (dtoHandle.Error != 0)
                return new BerkeleyResult<long>(dtoHandle.Error);

            return new BerkeleyResult<long>(0, Int64.Parse(dtoHandle.Result));
        }
        public override async Task<BerkeleyError> DeleteAsync(BerkeleyDb db, Byte[] key, BerkeleyDbDelete flag, BerkeleyDbMultiple multiple)
        {
            String requestUri = "api/database/delete/?handle=" + db.Handle.ToString() + "&flag=" + flag.ToStringEx() + "&multiple=" + multiple.ToStringEx();
            var content = new ObjectContent<Byte[]>(key, _formatter, (String)null);
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyResult<int>> GetPageSizeAsync(BerkeleyDb db)
        {
            String requestUri = "api/database/pagesize/?handle=" + db.Handle.ToString();
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyResult<int>(response.StatusCode);

            BerkeleyDtoResult dtoResult = await SerializeHelper.GetResultAsync(_serializer, response.Content).ConfigureAwait(false);
            if (dtoResult.Error != 0)
                return new BerkeleyResult<int>(dtoResult.Error);

            int pageSize = Int32.Parse(dtoResult.Result);
            return new BerkeleyResult<int>(pageSize);
        }
        public override async Task<BerkeleyError> OpenDbAsync(BerkeleyDb db, String name, BerkeleyDbOpenFlags flags)
        {
            var requesrUri = "api/database/open/?handle=" + db.Handle.ToString() + "&name=" + name + "&flags=" + flags.ToStringEx();
            HttpResponseMessage response = await _httpClient.GetAsync(requesrUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyResult<long>> OpenCursorAsync(BerkeleyDb db, BerkeleyDbCursorFlags flags)
        {
            String requestUri = "api/database/opencursor/?handle=" + db.Handle.ToString() + "&flags=" + flags.ToStringEx();
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyResult<long>(response.StatusCode);

            BerkeleyDtoResult result = await SerializeHelper.GetResultAsync(_serializer, response.Content).ConfigureAwait(false);
            if (result.Error != 0)
                return new BerkeleyResult<long>(result.Error);

            return new BerkeleyResult<long>(0, Int64.Parse(result.Result));
        }
        public override async Task<BerkeleyError> WriteAsync(BerkeleyDb db, Byte[] key, Byte[] value, BerkeleyDbWriteMode writeMode)
        {
            String requestUri = "api/database/write/?handle=" + db.Handle.ToString() + "&writemode=" + writeMode.ToStringEx();
            var data = new BerkeleyDtoPut(key, value);
            var content = new ObjectContent<BerkeleyDtoPut>(data, _formatter, (String)null);
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyError> WriteDuplicateAsync(BerkeleyDb db, BerkeleyDtoPut data, int bufferSize, BerkeleyDbWriteMode writeMode)
        {
            String requestUri;
            BerkeleyDtoPut dataPost;
            if (data.Key.Length <= bufferSize && data.Value.Length <= bufferSize)
            {
                requestUri = "api/database/writemultipleduplicate/?handle=" + db.Handle.ToString() + "&writemode=" + writeMode.ToStringEx();
                dataPost = new BerkeleyDtoPut(data.Key, data.Value);
            }
            else
            {
                requestUri = "api/database/write/?handle=" + db.Handle.ToString() + "&writemode=" + writeMode.ToStringEx();
                dataPost = data;
            }

            ObjectContent content = new ObjectContent<BerkeleyDtoPut>(dataPost, _formatter, (String)null);
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyError> WriteMultipleAsync(BerkeleyDb db, BerkeleyDtoPut data, BerkeleyDbWriteMode writeMode)
        {
            String requestUri;
            ObjectContent content;
            if (data.Key == null)
            {
                requestUri = "api/database/writemultiplekey/?handle=" + db.Handle.ToString() + "&writemode=" + writeMode.ToStringEx();
                content = new ObjectContent<Byte[]>(data.Value, _formatter, (String)null);
            }
            else
            {
                requestUri = "api/database/write/?handle=" + db.Handle.ToString() + "&writemode=" + writeMode.ToStringEx();
                content = new ObjectContent<BerkeleyDtoPut>(data, _formatter, (String)null);
            }

            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }

        public JsonMediaTypeFormatter Formatter
        {
            get
            {
                return _formatter;
            }
        }
        public HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }
        public JsonSerializer Serializer
        {
            get
            {
                return _serializer;
            }
        }
    }
}
