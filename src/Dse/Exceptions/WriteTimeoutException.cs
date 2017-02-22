//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Cassandra
{
    /// <summary>
    ///  A Cassandra timeout during a write query.
    /// </summary>
    public class WriteTimeoutException : QueryTimeoutException
    {
        public string WriteType { get; private set; }

        public WriteTimeoutException(ConsistencyLevel consistency, int received, int required,
                                     string writeType) :
                                         base(
                                         string.Format(
                                             "Cassandra timeout during write query at consistency {0} ({1} replica(s) acknowledged the write over {2} required)",
                                             consistency.ToString().ToUpper(), received, required),
                                         consistency,
                                         received,
                                         required)
        {
            WriteType = writeType;
        }
    }
}
