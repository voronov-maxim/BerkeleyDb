using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BerkeleyDbWebApiClient;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using BerkeleyDbClient;

namespace BerkeleyDbWebApiUnitTest
{
    [TestClass]
    public class BerkeleyDbWebApiClientTest
    {
        private static readonly Uri serviceRootUri = new Uri("http://localhost:9001/");

        [ClassInitialize]
        public static void RunOwinSelfHost(TestContext testContext)
        {
            OwinSelfHostHelper.Run();
        }
        [ClassCleanup]
        public static void StopOwinSelfHost()
        {
            OwinSelfHostHelper.Stop();
        }

        [TestMethod]
        public void BlobTest()
        {
            using (var bdb = CreateDbDup())
            using (var cursor = new BerkeleyBlobCursor(bdb, GetPageSize(bdb)))
            {
                int pageSize = GetPageSize(bdb);
                Byte[] key = Encoding.UTF8.GetBytes("partial");

                var error = cursor.WriteAsync(key, GenerateByteArray(37117)).Result;
                error.ThrowIfError();

                var reader = new BerkeleyBlobCursor(bdb, pageSize);
                var result = cursor.ReadAsync(key).Result;
                result.Error.ThrowIfError();

                Assert.IsTrue(CheckByteArray(result.Result), "data is bad");
            }
        }
        [TestMethod]
        public void BulkDiplicateTest()
        {
            using (var bdb = CreateDbDup())
            using (var cursor = new BerkeleyBulkDuplicateCursor(bdb, GetPageSize(bdb)))
            {
                bool blobExists;
                HashSet<KeyValuePair<int, DateTime>> data = BulkDuplicateWrite(cursor, out blobExists);
                BulkDuplicateRead(cursor, data, blobExists);
                data = BulkDuplicateDelete(cursor, data, ref blobExists);
                BulkDuplicateRead(cursor, data, blobExists);
            }
        }
        [TestMethod]
        public void BulkTest()
        {
            using (var bdb = CreateDb())
            using (var cursor = new BerkeleyBulkCursor(bdb, GetPageSize(bdb)))
            {
                bool blobExists;
                HashSet<int> keys = BulkWrite(cursor, out blobExists);
                BulkRead(cursor, keys, blobExists);
                keys = BulkDelete(cursor, keys, ref blobExists);
                BulkRead(cursor, keys, blobExists);
            }
        }
        [TestMethod]
        public void KeyValueTest()
        {
            using (var bdb = CreateDb())
            {
                var keys = new List<int>();
                for (int i = 0; i < 6; i++)
                {
                    keys.Add(i);
                    var key = Encoding.UTF8.GetBytes("key" + i.ToString());
                    var value = Encoding.UTF8.GetBytes("value" + i.ToString());
                    BerkeleyError error = bdb.WriteAsync(key, value, 0).Result;
                    error.ThrowIfError();
                }

                using (var cursor = new BerkeleyKeyValueCursor(bdb))
                {
                    KeyValueCheck(cursor, keys);

                    BerkeleyError error = cursor.DeleteCurrentAsync().Result;
                    error.ThrowIfError();

                    keys.Remove(keys.Last());
                    KeyValueCheck(cursor, keys);

                    var key = Encoding.UTF8.GetBytes("key3");
                    error = cursor.DeleteAsync(key, 0).Result;
                    error.ThrowIfError();

                    keys.Remove(3);
                    KeyValueCheck(cursor, keys);
                }
            }
        }

        private static HashSet<KeyValuePair<int, DateTime>> BulkDuplicateDelete(BerkeleyBulkDuplicateCursor cursor, HashSet<KeyValuePair<int, DateTime>> data, ref bool blobExists)
        {
            var checkData = new HashSet<KeyValuePair<int, DateTime>>();
            Byte[] key, value;
            foreach (KeyValuePair<int, DateTime> keyValue in data)
            {
                if ((keyValue.Value.Day % 3) == 0)
                {
                    key = Encoding.UTF8.GetBytes(keyValue.Key.ToString());
                    value = Encoding.UTF8.GetBytes(keyValue.Value.ToString("dddd dd MMMM yyyy"));
                    cursor.AddDelete(key, value);
                }
                else
                    checkData.Add(keyValue);
            }

            key = Encoding.UTF8.GetBytes((500 / 100 + 1).ToString());
            value = GenerateByteArray(34123);
            cursor.AddDelete(key, value);

            BerkeleyError error = cursor.DeleteAsync().Result;
            error.ThrowIfError();
            blobExists = false;
            return checkData;
        }
        private static void BulkDuplicateRead(BerkeleyBulkDuplicateCursor cursor, HashSet<KeyValuePair<int, DateTime>> data, bool blobExists)
        {
            var checkData = new HashSet<KeyValuePair<int, DateTime>>(data);
            bool blobFound = false;
            DateTime d1 = new DateTime(2014, 1, 1);

            foreach (int keyIndex in data.Select(d => d.Key).Distinct())
            {
                Byte[] key = Encoding.UTF8.GetBytes(keyIndex.ToString());
                for (BerkeleyBulkEnumerator buffer = cursor.ReadAsync(key, BerkeleyDbOperation.DB_SET).Result;
                    !buffer.NotFound;
                    buffer = cursor.ReadAsync(null, BerkeleyDbOperation.DB_NEXT_DUP).Result)
                {
                    buffer.Error.ThrowIfError();

                    foreach (BerkeleyKeyValueBulk keyValue in buffer)
                    {
                        String value = Encoding.UTF8.GetString(keyValue.Value.ToArray());
                        DateTime d;
                        if (DateTime.TryParseExact(value, "dddd dd MMMM yyyy", null, DateTimeStyles.None, out d))
                            Assert.IsTrue(checkData.Remove(new KeyValuePair<int, DateTime>(keyIndex, d)), "key/value: " + "key" + keyIndex + "/" + value + " not exists");
                        else
                        {
                            blobFound = keyValue.Value.Count == 34123 && CheckByteArray(keyValue.Value);
                            Assert.IsTrue(blobFound, "blod data is bad");
                        }
                    }
                }
            }

            Assert.AreEqual(checkData.Count, 0, "not all values found");
            Assert.IsTrue(blobFound == blobExists, "blob value not found");
        }
        private static HashSet<KeyValuePair<int, DateTime>> BulkDuplicateWrite(BerkeleyBulkDuplicateCursor cursor, out bool blobExists)
        {
            var data = new HashSet<KeyValuePair<int, DateTime>>();

            DateTime d1 = new DateTime(2014, 1, 1);
            for (int i = 0; i < 1000; i++)
            {
                int keyIndex = (i / 100) + 1;
                Byte[] key = Encoding.UTF8.GetBytes(keyIndex.ToString());
                if (i == 500)
                    cursor.AddWrite(key, GenerateByteArray(34123));

                String svalue = d1.AddDays(i).ToString("dddd dd MMMM yyyy");
                Byte[] value = Encoding.UTF8.GetBytes(svalue);
                cursor.AddWrite(key, value);

                data.Add(new KeyValuePair<int, DateTime>(keyIndex, d1.AddDays(i)));
            }

            var error = cursor.WriteAsync(BerkeleyDbWriteMode.DB_OVERWRITE_DUP).Result;
            error.ThrowIfError();

            blobExists = true;
            return data;
        }
        private static HashSet<int> BulkDelete(BerkeleyBulkCursor cursor, HashSet<int> keys, ref bool blobExists)
        {
            foreach (int i in keys.Where(i => (i % 3) == 0))
                cursor.AddDelete(Encoding.UTF8.GetBytes(i.ToString()));
            if (blobExists)
                cursor.AddDelete(Encoding.UTF8.GetBytes("blob value"));

            BerkeleyError error = cursor.DeleteAsync().Result;
            error.ThrowIfError();

            blobExists = false;
            return new HashSet<int>(keys.Where(i => (i % 3) != 0));
        }
        private static void BulkRead(BerkeleyBulkCursor cursor, HashSet<int> keys, bool blobExists)
        {
            var checkKeys = new HashSet<int>(keys);
            bool blobFound = false;
            DateTime d1 = new DateTime(2014, 1, 1);

            for (BerkeleyBulkEnumerator buffer = cursor.ReadAsync(Encoding.UTF8.GetBytes("1"), BerkeleyDbOperation.DB_SET).Result;
                !buffer.NotFound;
                buffer = cursor.ReadAsync(null, BerkeleyDbOperation.DB_NEXT).Result)
            {
                buffer.Error.ThrowIfError();

                foreach (BerkeleyKeyValueBulk data in buffer)
                {
                    String key = Encoding.UTF8.GetString(data.Key.ToArray());
                    int i;
                    if (int.TryParse(key, out i))
                    {
                        Assert.IsTrue(checkKeys.Remove(i), "key: " + i + " not exists");

                        String value = Encoding.UTF8.GetString(data.Value.ToArray());
                        Assert.AreEqual(DateTime.ParseExact(value, "dddd dd MMMM yyyy", null), d1.AddDays(i - 1), "bad value: " + value);
                    }
                    else
                        blobFound = key == "blob value" && data.Value.Count == 34123 && CheckByteArray(data.Value);
                }
            }

            Assert.AreEqual(checkKeys.Count, 0, "not all keys found");
            Assert.IsTrue(blobFound == blobExists, "blob error");
        }
        private static HashSet<int> BulkWrite(BerkeleyBulkCursor cursor, out bool blobExists)
        {
            var keys = new HashSet<int>();
            DateTime d1 = new DateTime(2014, 1, 1);
            for (int i = 0; i < 3000; i++)
            {
                if (i == 500)
                {
                    Byte[] key2 = Encoding.UTF8.GetBytes("blob value");
                    Byte[] value2 = GenerateByteArray(34123);
                    cursor.AddWrite(key2, value2);
                }

                Byte[] key = Encoding.UTF8.GetBytes((i + 1).ToString());
                String v = d1.AddDays(i).ToString("dddd dd MMMM yyyy");
                Byte[] value = Encoding.UTF8.GetBytes(v);
                cursor.AddWrite(key, value);
                keys.Add(i + 1);
            }
            var error = cursor.WriteAsync(BerkeleyDbWriteMode.DB_OVERWRITE_DUP).Result;
            error.ThrowIfError();

            blobExists = true;
            return keys;
        }

        private static bool CheckByteArray(IList<Byte> data)
        {
            for (int i = 0; i < data.Count / 4; i++)
            {
                long y = i * i - 31;
                Byte[] source = BitConverter.GetBytes(y);
                int count = Math.Min(data.Count - i * 4, 4);
                for (int j = 0; j < count; j++)
                    if (data[i * 4 + j] != source[j])
                        return false;
            }

            return true;
        }
        private static BerkeleyDb CreateDb()
        {
            return CreateDb(0);
        }
        private static BerkeleyDb CreateDb(BerkeleyDbFlags flags)
        {
            var client = new HttpClient();
            client.BaseAddress = serviceRootUri;
            var methods = new BerkeleyDbWebApiMethods(client, new Newtonsoft.Json.JsonSerializer(), new System.Net.Http.Formatting.JsonMediaTypeFormatter());
            var bdb = new BerkeleyDb(methods, BerkeleyDbType.DB_BTREE, flags);

            String fileName = Path.Combine(Path.GetTempPath(), "test.bdb");
            File.Delete(fileName);
            BerkeleyError error = bdb.OpenAsync(fileName, BerkeleyDbOpenFlags.DB_CREATE).Result;
            error.ThrowIfError();

            return bdb;
        }
        private static BerkeleyDb CreateDbDup()
        {
            return CreateDb(BerkeleyDbFlags.DB_DUP);
        }
        private static Byte[] GenerateByteArray(int size)
        {
            var data = new Byte[size];
            for (int i = 0; i < size / 4; i++)
            {
                long y = i * i - 31;
                Byte[] source = BitConverter.GetBytes(y);
                int count = Math.Min(size - i * 4, 4);
                Buffer.BlockCopy(source, 0, data, i * 4, count);
            }
            return data;
        }
        private static int GetPageSize(BerkeleyDb bdb)
        {
            BerkeleyResult<int> pageSizeResult = bdb.GetPageSizeAsync().Result;
            pageSizeResult.Error.ThrowIfError();
            return pageSizeResult.Result;
        }
        private static void KeyValueCheck(BerkeleyKeyValueCursor cursor, List<int> keys)
        {
            var readKeys = new List<int>();
            for (BerkeleyResult<BerkeleyKeyValue> result = cursor.ReadAsync(null, BerkeleyDbOperation.DB_FIRST).Result;
                result.Error.BerkeleyDbError != BerkeleyDbError.DB_NOTFOUND;
                result = cursor.ReadAsync(null, BerkeleyDbOperation.DB_NEXT).Result)
            {
                result.Error.ThrowIfError();

                String skey = Encoding.UTF8.GetString(result.Result.Key);
                int ikey = int.Parse(skey.Replace("key", ""));

                String svalue = Encoding.UTF8.GetString(result.Result.Value);
                int ivalue = int.Parse(svalue.Replace("value", ""));

                Assert.IsTrue(ikey == ivalue, "read error");

                readKeys.Add(ikey);
            }

            Assert.IsTrue(keys.SequenceEqual(readKeys), "delete error");
        }
    }
}
