//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassandra
{
    /// <summary>
    /// A representation of a secondary index in Cassandra
    /// </summary>
    public class IndexMetadata
    {
        /// <summary>
        /// Describes the possible kinds of indexes
        /// </summary>
        public enum IndexKind
        {
            Keys,
            Custom,
            Composites
        }

        private static IndexKind GetKindByName(string name)
        {
            IndexKind result;
            if (!Enum.TryParse(name, true, out result))
            {
                return IndexKind.Custom;
            }
            return result;
        }

        /// <summary>
        /// Gets the index name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the index target
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Gets the index kind
        /// </summary>
        public IndexKind Kind { get; private set; }

        /// <summary>
        /// Gets index options
        /// </summary>
        public IDictionary<string, string> Options { get; private set; }

        public IndexMetadata(string name, string target, IndexKind kind, IDictionary<string, string> options)
        {
            Name = name;
            Target = target;
            Kind = kind;
            Options = options;
        }

        /// <summary>
        /// From legacy columns
        /// </summary>
        internal static IndexMetadata FromTableColumn(TableColumn c)
        {
            //using obsolete properties
            #pragma warning disable 618
            string target = null;
            if (c.SecondaryIndexOptions.ContainsKey("index_keys"))
            {
                target = string.Format("keys({0})", c.Name);
            }
            else if (c.SecondaryIndexOptions.ContainsKey("index_keys_and_values"))
            {
                target = string.Format("entries({0})", c.Name);
            }
            else if (c.TypeCode == ColumnTypeCode.List || c.TypeCode == ColumnTypeCode.Set || c.TypeCode == ColumnTypeCode.Map)
            {
                target = string.Format("values({0})", c.Name);
            }
            return new IndexMetadata(c.SecondaryIndexName, target, GetKindByName(c.SecondaryIndexType), c.SecondaryIndexOptions);
            #pragma warning restore 618
        }

        /// <summary>
        /// From a row in the 'system_schema.indexes' table
        /// </summary>
        internal static IndexMetadata FromRow(Row row)
        {
            var options = row.GetValue<IDictionary<string, string>>("options");
            return new IndexMetadata(row.GetValue<string>("index_name"), options["target"], GetKindByName(row.GetValue<string>("kind")), options);
        }
    }
}
