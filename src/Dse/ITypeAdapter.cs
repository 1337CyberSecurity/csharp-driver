//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;

namespace Cassandra
{
    /// <summary>
    /// DEPRECATED, use <see cref="Cassandra.Serialization.TypeSerializer{T}"/> instead.
    /// Represents a adapter to convert a Cassandra type to a CLR type.
    /// </summary>
    public interface ITypeAdapter
    {
        Type GetDataType();
        object ConvertFrom(byte[] decimalBuf);
        byte[] ConvertTo(object value);
    }
}