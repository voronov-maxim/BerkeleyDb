using BerkeleyDbClient;
using BerkeleyDbWebApiClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestBerkeleyDbWebApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceRootUri = new Uri("http://localhost:9001/");

            using (var client = new HttpClient())
            {
                client.BaseAddress = serviceRootUri;

                var methods = new BerkeleyDbWebApiMethods(client, new JsonSerializer(), new System.Net.Http.Formatting.JsonMediaTypeFormatter());
                using (var bdb = new BerkeleyDb(methods, BerkeleyDbType.DB_BTREE, BerkeleyDbFlags.DB_DUP))
                {
                    String fileName = Path.Combine(Path.GetTempPath(), "test.bdb");
                    File.Delete(fileName);
                    bdb.OpenAsync("test", BerkeleyDbOpenFlags.DB_CREATE).Wait();

                    var keys = new List<int>();
                    for (int i = 0; i < 6; i++)
                    {
                        keys.Add(i);
                        var key = Encoding.UTF8.GetBytes("key" + i.ToString());
                        var value = Encoding.UTF8.GetBytes("value" + i.ToString());
                        var error = bdb.WriteAsync(key, value, 0).Result;
                        error.ThrowIfError();
                    }

                    using (var readCursor = new BerkeleyKeyValueCursor(bdb))
                    {
                        ReadAll(readCursor);
                        bool f = Check(readCursor, keys);

                        //var error = cursor.DeleteCurrentAsync().Result;
                        //error.ThrowIfError();

                        //keys.Remove(keys.Last());
                        //f = Check(cursor, keys);

                        //var key = Encoding.UTF8.GetBytes("key3");
                        //error = cursor.DeleteAsync(key, 0).Result;
                        //error.ThrowIfError();

                        //keys.Remove(3);
                        //f = Check(cursor, keys);

                        //Console.WriteLine();
                        //ReadAll(cursor);

                        using (var delCursor = new BerkeleyBulkCursor(bdb, bdb.GetPageSizeAsync().Result.Result))
                        {
                            for (int i = 1; i < 5; i++)
                            {
                                keys.Remove(i);
                                var key = Encoding.UTF8.GetBytes("key" + i.ToString());
                                delCursor.AddDelete(key);
                            }

                            var error = delCursor.DeleteAsync().Result;
                            error.ThrowIfError();
                        }

                        Console.WriteLine();
                        ReadAll(readCursor);
                        f = Check(readCursor, keys);
                    }
                    //WriteFoto(bdb, @"D:\гуманитарий с дипломом менеджера.jpg");
                    //ReadFoto(bdb);
                }
            }
        }

        private static void ReadAll(BerkeleyKeyValueCursor cursor)
        {
            for (BerkeleyResult<BerkeleyKeyValue> result = cursor.ReadAsync(null, BerkeleyDbOperation.DB_FIRST).Result;
                result.Error.BerkeleyDbError != BerkeleyDbError.DB_NOTFOUND;
                result = cursor.ReadAsync(null, BerkeleyDbOperation.DB_NEXT).Result)
            {
                result.Error.ThrowIfError();
                WriteLine(result.Result.Key, result.Result.Value);
            }
        }
        private static bool Check(BerkeleyKeyValueCursor cursor, List<int> keys)
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

                if (ikey != ivalue)
                    throw new InvalidOperationException();

                readKeys.Add(ikey);
            }

            return keys.SequenceEqual(readKeys);
        }
        private static void WriteLine(Byte[] key, Byte[] value)
        {
            String skey = Encoding.UTF8.GetString(key);
            String svalue = Encoding.UTF8.GetString(value);
            Console.WriteLine(String.Format("key: {0}, value: {1}", skey, svalue));
        }
        private static void WriteFoto(BerkeleyDb bdb, String fileName)
        {
            Byte[] key = System.Text.Encoding.UTF8.GetBytes(("simple key/value").ToString());
            Byte[] value = File.ReadAllBytes(fileName);
            var err = bdb.WriteAsync(key, value, 0).Result;
        }
        private static void ReadFoto(BerkeleyDb bdb)
        {
            Byte[] key = System.Text.Encoding.UTF8.GetBytes("simple key/value");
            using (var reader = new BerkeleyKeyValueCursor(bdb))
            {
                var result = reader.ReadAsync(key, BerkeleyDbOperation.DB_SET).Result;
                File.WriteAllBytes(@"D:\zzz.jpg", result.Result.Value.ToArray());
            }
        }
    }
}
