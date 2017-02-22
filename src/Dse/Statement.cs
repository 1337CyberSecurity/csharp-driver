//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Threading;
using Cassandra.Requests;

namespace Cassandra
{
    /// <summary>
    /// Base class for statements that contains the options.
    /// </summary>
    public abstract class Statement : IStatement
    {
        private ConsistencyLevel _serialConsistency = QueryProtocolOptions.Default.SerialConsistency;
        private object[] _values;
        private bool _autoPage = true;
        private volatile int _isIdempotent = int.MinValue;

        public virtual object[] QueryValues
        {
            get { return _values; }
        }
        /// <inheritdoc />
        public bool SkipMetadata { get; private set; }

        /// <inheritdoc />
        public ConsistencyLevel? ConsistencyLevel { get; private set; }

        /// <summary>
        /// Gets the serial consistency level for this query.
        /// </summary>        
        public ConsistencyLevel SerialConsistencyLevel
        {
            get { return _serialConsistency; }
        }

        /// <inheritdoc />
        public int PageSize { get; private set; }

        /// <inheritdoc />
        public bool IsTracing { get; private set; }

        /// <inheritdoc />
        public int ReadTimeoutMillis { get; private set; }

        /// <inheritdoc />
        public IRetryPolicy RetryPolicy { get; private set; }

        /// <inheritdoc />
        public byte[] PagingState { get; private set; }

        /// <inheritdoc />
        public DateTimeOffset? Timestamp { get; private set; }

        /// <inheritdoc />
        public bool AutoPage
        {
            get { return _autoPage; }
        }

        /// <inheritdoc />
        public IDictionary<string, byte[]> OutgoingPayload { get; private set; }

        /// <inheritdoc />
        public abstract RoutingKey RoutingKey { get; }

        /// <inheritdoc />
        public bool? IsIdempotent
        {
            get
            {
                var idempotence = _isIdempotent;
                if (idempotence == int.MinValue)
                {
                    return null;
                }
                return idempotence == 1;
            }
        }

        protected Statement()
        {

        }

        // ReSharper disable once UnusedParameter.Local
        protected Statement(QueryProtocolOptions queryProtocolOptions)
        {
            //the unused parameter is maintained for backward compatibility
        }

        /// <inheritdoc />
        internal Statement SetSkipMetadata(bool val)
        {
            SkipMetadata = val;
            return this;
        }

        /// <summary>
        ///  Bound values to the variables of this statement. This method provides a
        ///  convenience to bound all the variables of the <c>BoundStatement</c> in
        ///  one call.
        /// </summary>
        /// <param name="values"> the values to bind to the variables of the newly
        ///  created BoundStatement. The first element of <c>values</c> will 
        ///  be bound to the first bind variable,
        ///  etc.. It is legal to provide less values than the statement has bound
        ///  variables. In that case, the remaining variable need to be bound before
        ///  execution. If more values than variables are provided however, an
        ///  IllegalArgumentException will be raised. </param>
        /// <returns>this bound statement. </returns>
        internal virtual void SetValues(object[] values)
        {
            _values = values;
        }

        /// <inheritdoc />
        public IStatement SetAutoPage(bool autoPage)
        {
            _autoPage = autoPage;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetPagingState(byte[] pagingState)
        {
            PagingState = pagingState;
            //Disable automatic paging only if paging state is set to something other then null
            if (pagingState != null && pagingState.Length > 0)
            {
                return SetAutoPage(false);
            }
            return this;
        }

        /// <inheritdoc />
        public IStatement SetReadTimeoutMillis(int timeout)
        {
            ReadTimeoutMillis = timeout;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetConsistencyLevel(ConsistencyLevel? consistency)
        {
            ConsistencyLevel = consistency;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetSerialConsistencyLevel(ConsistencyLevel serialConsistency)
        {
            if (serialConsistency.IsSerialConsistencyLevel() == false)
            {
                throw new ArgumentException("The serial consistency can only be set to ConsistencyLevel.LocalSerial or ConsistencyLevel.Serial.");
            }
            _serialConsistency = serialConsistency;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetTimestamp(DateTimeOffset value)
        {
            Timestamp = value;
            return this;
        }

        /// <inheritdoc />
        public IStatement EnableTracing(bool enable = true)
        {
            IsTracing = enable;
            return this;
        }

        /// <inheritdoc />
        public IStatement DisableTracing()
        {
            IsTracing = false;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetRetryPolicy(IRetryPolicy policy)
        {
            RetryPolicy = policy;
            return this;
        }

        internal virtual IQueryRequest CreateBatchRequest(ProtocolVersion protocolVersion)
        {
            throw new InvalidOperationException("Cannot insert this query into the batch");
        }
        
        /// <inheritdoc />
        public IStatement SetIdempotence(bool value)
        {
            _isIdempotent = value ? 1 : 0;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetPageSize(int pageSize)
        {
            PageSize = pageSize;
            return this;
        }

        /// <inheritdoc />
        public IStatement SetOutgoingPayload(IDictionary<string, byte[]> payload)
        {
            OutgoingPayload = payload;
            return this;
        }
    }
}
