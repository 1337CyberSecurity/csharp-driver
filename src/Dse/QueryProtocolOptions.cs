//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using Cassandra.Serialization;

namespace Cassandra
{
    public class QueryProtocolOptions
    {
        //This class was leaked to the API, making it internal would be a breaking change
        [Flags]
        public enum QueryFlags
        {
            Values = 0x01,
            SkipMetadata = 0x02,
            PageSize = 0x04,
            WithPagingState = 0x08,
            WithSerialConsistency = 0x10,
            WithDefaultTimestamp = 0x20,
            WithNameForValues = 0x40
        }

        public static readonly QueryProtocolOptions Default = 
            new QueryProtocolOptions(ConsistencyLevel.One, null, false, QueryOptions.DefaultPageSize, null, ConsistencyLevel.Any);

        private readonly bool _skipMetadata;
        public readonly int PageSize;
        public readonly ConsistencyLevel SerialConsistency;

        public byte[] PagingState { get; set; }
        public object[] Values { get; private set; }
        public ConsistencyLevel Consistency { get; set; }
        public DateTimeOffset? Timestamp { get; private set; }
        /// <summary>
        /// Names of the query parameters
        /// </summary>
        public IList<string> ValueNames { get; set; }

        internal QueryProtocolOptions(ConsistencyLevel consistency,
                                      object[] values,
                                      bool skipMetadata,
                                      int pageSize,
                                      byte[] pagingState,
                                      ConsistencyLevel serialConsistency)
        {
            Consistency = consistency;
            Values = values;
            _skipMetadata = skipMetadata;
            if (pageSize <= 0)
            {
                PageSize = QueryOptions.DefaultPageSize;
            }
            else if (pageSize == int.MaxValue)
            {
                PageSize = -1;
            }
            else
            {
                PageSize = pageSize;
            }
            PagingState = pagingState;
            SerialConsistency = serialConsistency;
        }

        internal static QueryProtocolOptions CreateFromQuery(Statement query, QueryOptions queryOptions)
        {
            if (query == null)
            {
                return Default;
            }
            var consistency = query.ConsistencyLevel ?? queryOptions.GetConsistencyLevel();
            var pageSize = query.PageSize != 0 ? query.PageSize : queryOptions.GetPageSize();
            var options = new QueryProtocolOptions(
                consistency,
                query.QueryValues,
                query.SkipMetadata, 
                pageSize, 
                query.PagingState, 
                query.SerialConsistencyLevel)
            {
                Timestamp = query.Timestamp
            };
            return options;
        }

        private QueryFlags GetFlags()
        {
            QueryFlags flags = 0;
            if (Values != null && Values.Length > 0)
            {
                flags |= QueryFlags.Values;
            }
            if (_skipMetadata)
            {
                flags |= QueryFlags.SkipMetadata;
            }
            if (PageSize != int.MaxValue && PageSize >= 0)
            {
                flags |= QueryFlags.PageSize;
            }
            if (PagingState != null)
            {
                flags |= QueryFlags.WithPagingState;
            }
            if (SerialConsistency != ConsistencyLevel.Any)
            {
                flags |= QueryFlags.WithSerialConsistency;
            }
            if (Timestamp != null)
            {
                flags |= QueryFlags.WithDefaultTimestamp;
            }
            if (ValueNames != null && ValueNames.Count > 0)
            {
                flags |= QueryFlags.WithNameForValues;
            }
            return flags;
        }

        //TODO: Move to ExecuteRequest and QueryRequest
        internal void Write(FrameWriter wb, bool isPrepared)
        {
            //protocol v1: <query><n><value_1>....<value_n><consistency>
            //protocol v2: <query><consistency><flags>[<n><value_1>...<value_n>][<result_page_size>][<paging_state>][<serial_consistency>]
            //protocol v3: <query><consistency><flags>[<n>[name_1]<value_1>...[name_n]<value_n>][<result_page_size>][<paging_state>][<serial_consistency>][<timestamp>]
            var protocolVersion = wb.Serializer.ProtocolVersion;
            var flags = GetFlags();

            if (protocolVersion != ProtocolVersion.V1)
            {
                wb.WriteUInt16((ushort)Consistency);
                wb.WriteByte((byte)flags);
            }
            if (flags.HasFlag(QueryFlags.Values))
            {
                wb.WriteUInt16((ushort)Values.Length);
                for (var i = 0; i < Values.Length; i++)
                {
                    if (flags.HasFlag(QueryFlags.WithNameForValues))
                    {
                        var name = ValueNames[i];
                        wb.WriteString(name);
                    }
                    wb.WriteAsBytes(Values[i]);
                }
            }
            else if (protocolVersion == ProtocolVersion.V1 && isPrepared)
            {
                //n values is not optional on protocol v1
                //Write 0 values
                wb.WriteUInt16(0);
            }

            if (protocolVersion == ProtocolVersion.V1)
            {
                //Protocol v1 ends here
                wb.WriteUInt16((ushort)Consistency);
                return;
            }
            if ((flags & QueryFlags.PageSize) == QueryFlags.PageSize)
            {
                wb.WriteInt32(PageSize);
            }
            if ((flags & QueryFlags.WithPagingState) == QueryFlags.WithPagingState)
            {
                wb.WriteBytes(PagingState);
            }
            if ((flags & QueryFlags.WithSerialConsistency) == QueryFlags.WithSerialConsistency)
            {
                wb.WriteUInt16((ushort)SerialConsistency);
            }
            if (Timestamp != null)
            {
                //Expressed in microseconds
                wb.WriteLong(TypeSerializer.SinceUnixEpoch(Timestamp.Value).Ticks / 10);
            }
        }
    }
}