﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Cassandra
{

    internal partial class TypeInterpreter
    {
        public static object ConvertFromMap(TableMetadata.ColumnInfo type_info, byte[] value)
        {
            if (type_info is TableMetadata.MapColumnInfo)
            {
                var key_typecode = (type_info as TableMetadata.MapColumnInfo).KeyTypeCode;
                var key_typeinfo = (type_info as TableMetadata.MapColumnInfo).KeyTypeInfo;
                var value_typecode = (type_info as TableMetadata.MapColumnInfo).ValueTypeCode;
                var value_typeinfo = (type_info as TableMetadata.MapColumnInfo).ValueTypeInfo;
                var key_type = TypeInterpreter.GetTypeFromCqlType(key_typecode, key_typeinfo);
                var value_type = TypeInterpreter.GetTypeFromCqlType(value_typecode, value_typeinfo);
                int count = ConversionHelper.FromBytestToInt16(value, 0);
                int idx = 2;
                var openType = typeof(SortedDictionary<,>);
                var dicType = openType.MakeGenericType(key_type, value_type);
                object ret = Activator.CreateInstance(dicType);
                var addM = dicType.GetMethod("Add");
                for (int i = 0; i < count; i++)
                {
                    var key_buf_len = ConversionHelper.FromBytestToInt16(value, idx);
                    idx += 2;
                    byte[] key_buf = new byte[key_buf_len];
                    Buffer.BlockCopy(value, idx, key_buf, 0, key_buf_len);
                    idx += key_buf_len;

                    var value_buf_len = ConversionHelper.FromBytestToInt16(value, idx);
                    idx += 2;
                    byte[] value_buf = new byte[value_buf_len];
                    Buffer.BlockCopy(value, idx, value_buf, 0, value_buf_len);
                    idx += value_buf_len;

                    addM.Invoke(ret, new object[] {
                        TypeInterpreter.CqlConvert(key_buf, key_typecode, key_typeinfo),
                        TypeInterpreter.CqlConvert(value_buf, value_typecode, value_typeinfo)
                    });
                }
                return ret;
            }
            throw new DriverInternalError("Invalid ColumnInfo");
        }

        public static Type GetTypeFromMap(TableMetadata.ColumnInfo type_info)
        {
            if (type_info is TableMetadata.MapColumnInfo)
            {
                var key_typecode = (type_info as TableMetadata.MapColumnInfo).KeyTypeCode;
                var key_typeinfo = (type_info as TableMetadata.MapColumnInfo).KeyTypeInfo;
                var value_typecode = (type_info as TableMetadata.MapColumnInfo).ValueTypeCode;
                var value_typeinfo = (type_info as TableMetadata.MapColumnInfo).ValueTypeInfo;
                var key_type = TypeInterpreter.GetTypeFromCqlType(key_typecode, key_typeinfo);
                var value_type = TypeInterpreter.GetTypeFromCqlType(value_typecode, value_typeinfo);

                var kvType = typeof(KeyValuePair<,>);
                var openType = typeof(IEnumerable<>);
                var dicType = openType.MakeGenericType(kvType.MakeGenericType(key_type, value_type));
                return dicType;
            }
            throw new DriverInternalError("Invalid ColumnInfo");
        }

        public static byte[] InvConvertFromMap(TableMetadata.ColumnInfo type_info, object value)
        {
            var dicType = GetTypeFromList(type_info);
            CheckArgument(dicType, value);
            var key_typecode = (type_info as TableMetadata.MapColumnInfo).KeyTypeCode;
            var key_typeinfo = (type_info as TableMetadata.MapColumnInfo).KeyTypeInfo;
            var value_typecode = (type_info as TableMetadata.MapColumnInfo).ValueTypeCode;
            var value_typeinfo = (type_info as TableMetadata.MapColumnInfo).ValueTypeInfo;
            var key_type = TypeInterpreter.GetTypeFromCqlType(key_typecode, key_typeinfo);
            var value_type = TypeInterpreter.GetTypeFromCqlType(value_typecode, value_typeinfo);

            List<byte[]> bufs = new List<byte[]>();
            int cnt = 0;
            int bsize = 2;
                var kvoType = typeof(KeyValuePair<,>);
            var kvType  =kvoType.MakeGenericType(key_type, value_type);
            var key_prop = kvType.GetProperty("Key");
            var value_prop = kvType.GetProperty("Value");
            foreach (var kv in (value as IEnumerable))
            {
                {
                    var obj = key_prop.GetValue(kv, new object[] { });
                    var buf = TypeInterpreter.InvCqlConvert(obj, key_typecode, key_typeinfo);
                    bufs.Add(buf);
                    bsize += buf.Length;
                }
                {
                    var obj = value_prop.GetValue(kv, new object[] { });
                    var buf = TypeInterpreter.InvCqlConvert(obj, value_typecode, value_typeinfo);
                    bufs.Add(buf);
                    bsize += buf.Length;
                }
                cnt++;
            }
            var ret = new byte[bsize];

            var cntbuf = ConversionHelper.ToBytesFromInt16((short)cnt); // short or ushort ? 

            int idx = 0;
            Buffer.BlockCopy(cntbuf, 0, ret, 0, 2);
            idx += 2;
            foreach (var buf in bufs)
            {
                Buffer.BlockCopy(buf, 0, ret, idx, buf.Length);
                idx += buf.Length;
            }

            return ret;
        }
    }
}
