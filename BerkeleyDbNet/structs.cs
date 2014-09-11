using BerkeleyDbClient;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BerkeleyDbNet
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct db_dbt : IDisposable
    {
        public const uint DB_DBT_BULK = 0x0002;
        public const uint DB_DBT_PARTIAL = 0x0040;
        public const uint DB_DBT_READONLY = 0x0100;
        public const uint DB_DBT_STREAMING = 0x0200;
        public const uint DB_DBT_USERMEM = 0x0800;
        public const uint DB_DBT_BLOB = 0x1000;

        public IntPtr data;
        public uint size;
        public uint ulen;
        public uint dlen;
        public uint doff;
        public IntPtr app_data;
        public uint flags;

        public void CopyToArray(Byte[] buffer)
        {
            Marshal.Copy(data, buffer, 0, (int)size);
        }
        public void Dispose()
        {
            Marshal.FreeHGlobal(data);
        }
        public void Init(int length)
        {
            data = Marshal.AllocHGlobal(length);
            flags = DB_DBT_USERMEM;
            size = (uint)length;
            ulen = (uint)length;
        }
        public void Init(Byte[] data)
        {
            Init(data.Length);
            Marshal.Copy(data, 0, this.data, data.Length);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct db_bt_stat
    {
        uint bt_magic;		/* Magic number. */
        uint bt_version;		/* Version number. */
        uint bt_metaflags;		/* Metadata flags. */
        uint bt_nkeys;		/* Number of unique keys. */
        uint bt_ndata;		/* Number of data items. */
        uint bt_pagecnt;		/* Page count. */
        uint bt_pagesize;		/* Page size. */
        uint bt_minkey;		/* Minkey value. */
        uint bt_nblobs;		/* Number of blobs. */
        uint bt_re_len;		/* Fixed-length record length. */
        uint bt_re_pad;		/* Fixed-length record pad. */
        uint bt_levels;		/* Tree levels. */
        uint bt_int_pg;		/* Internal pages. */
        uint bt_leaf_pg;		/* Leaf pages. */
        uint bt_dup_pg;		/* Duplicate pages. */
        uint bt_over_pg;		/* Overflow pages. */
        uint bt_empty_pg;		/* Empty pages. */
        uint bt_free;		/* Pages on the free list. */

        UIntPtr bt_int_pgfree;	/* Bytes free in internal pages. */
        UIntPtr bt_leaf_pgfree;	/* Bytes free in leaf pages. */
        UIntPtr bt_dup_pgfree;	/* Bytes free in duplicate pages. */
        UIntPtr bt_over_pgfree;	/* Bytes free in overflow pages. */
    };

}
