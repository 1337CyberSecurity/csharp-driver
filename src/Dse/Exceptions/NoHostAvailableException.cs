//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Cassandra
{
    /// <summary>
    ///  Exception thrown when a query cannot be performed because no host are
    ///  available. This exception is thrown if <ul> <li>either there is no host live
    ///  in the cluster at the moment of the query</li> <li>all host that have been
    ///  tried have failed due to a connection problem</li> </ul> For debugging
    ///  purpose, the list of hosts that have been tried along with the failure cause
    ///  can be retrieved using the <link>#errors</link> method.
    /// </summary>
#if !NETCORE
    [Serializable]
#endif
    public class NoHostAvailableException : DriverException
    {
        /// <summary>
        ///  Gets the hosts tried along with descriptions of the error encountered while trying them. 
        /// </summary>
        public Dictionary<IPEndPoint, Exception> Errors { get; private set; }

        public NoHostAvailableException(Dictionary<IPEndPoint, Exception> errors)
            : base(MakeMessage(errors))
        {
            Errors = errors;
        }

#if !NETCORE
        protected NoHostAvailableException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            
        }
#endif

        private static String MakeMessage(Dictionary<IPEndPoint, Exception> errors)
        {
            return string.Format("None of the hosts tried for query are available (tried: {0})", String.Join(",", errors.Keys.Select((ip) => ip.ToString())));
        }
    }
}
