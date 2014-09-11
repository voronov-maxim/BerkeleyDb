using BerkeleyDbClient;
using BerkeleyDbClient.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace BerkeleyDbWebApiClient
{
    public sealed class BerkeleyCursorWebApiMethods : BerkeleyCursorMethodsAsync
    {
        private readonly HttpClient _httpClient;
        private readonly JsonMediaTypeFormatter _formatter;
        private readonly JsonSerializer _serializer;

        public BerkeleyCursorWebApiMethods(BerkeleyDbWebApiMethods methods, JsonMediaTypeFormatter formatter)
        {
            _httpClient = methods.HttpClient;
            _serializer = methods.Serializer;
            _formatter = formatter;
        }

        public override async Task<BerkeleyError> CloseCursorAsync(BerkeleyCursor cursor)
        {
            String requestUri = "api/cursor/close/?handle=" + cursor.Handle.ToString();
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyError> DeleteCurrentAsync(BerkeleyCursor cursor, BerkeleyDbDelete flag)
        {
            String requestUri = "api/cursor/deletecurrent/?handle=" + cursor.Handle.ToString() + "&flag=" + flag.ToStringEx();
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            BerkeleyDbError error = await SerializeHelper.GetErrorAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyError(error);
        }
        public override async Task<BerkeleyResult<BerkeleyDtoGet>> GetDtoGet(BerkeleyCursor cursor, Byte[] key, BerkeleyDbOperation operation, BerkeleyDbMultiple multiple, int bufferSize)
        {
            var requestUri = "api/cursor/read/?handle=" + cursor.Handle.ToString() + "&operation=" + operation.ToStringEx() + "&multiple=" + multiple.ToStringEx() + "&size=" + bufferSize.ToString();
            var content = new ObjectContent<Byte[]>(key, _formatter, (String)null);
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyResult<BerkeleyDtoGet>(response.StatusCode);

            BerkeleyDtoGet berkeleyDataGet = await SerializeHelper.GetDataGetAsync(_serializer, response.Content).ConfigureAwait(false);
            return new BerkeleyResult<BerkeleyDtoGet>((BerkeleyDbError)berkeleyDataGet.ErrorCode, berkeleyDataGet);
        }
        public override async Task<BerkeleyResult<Byte[]>> ReadPartialAsync(BerkeleyCursor cursor, Byte[] key, int offset, int bufferSize)
        {
            ObjectContent content = null;
            if (offset == 0)
                content = new ObjectContent<Byte[]>(key, _formatter, (String)null);

            String requestUri = "api/cursor/readpartial/?handle=" + cursor.Handle.ToString() + "&valueOffset=" + offset.ToString() + "&size=" + bufferSize.ToString();
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyResult<Byte[]>(response.StatusCode);

            BerkeleyDtoGet dataGet = await SerializeHelper.GetDataGetAsync(_serializer, response.Content).ConfigureAwait(false);
            if (dataGet.ErrorCode == 0)
                return new BerkeleyResult<Byte[]>(dataGet.Value);
            else
                return new BerkeleyResult<Byte[]>((BerkeleyDbError)dataGet.ErrorCode);
        }
        public override async Task<BerkeleyError> WritePartialAsync(BerkeleyCursor cursor, Byte[] key, Byte[] value, int offset, int bufferSize)
        {
            JToken jtoken = SerializeHelper.CreateJTokenDtoPartialPut(offset == 0 ? key : null, value, bufferSize, offset);
            var content = new ObjectContent<JToken>(jtoken, _formatter, (String)null);

            HttpResponseMessage response = await _httpClient.PostAsync("api/cursor/writepartial/?handle=" + cursor.Handle.ToString(), content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new BerkeleyError(response.StatusCode);

            String errorString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            BerkeleyDbError error = (BerkeleyDbError)Int32.Parse(errorString);
            return new BerkeleyError(error);
        }
    }
}
