//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra.Tasks;

namespace Cassandra.Data.Linq
{
    public abstract class Batch : Statement
    {
        protected readonly ISession _session;

        protected BatchType _batchType = BatchType.Logged;
        protected DateTimeOffset? _timestamp = null;

        public abstract bool IsEmpty { get; }

        public override RoutingKey RoutingKey
        {
            get { return null; }
        }

        public QueryTrace QueryTrace { get; private set; }

        internal Batch(ISession session)
        {
            _session = session;
        }

        public abstract void Append(CqlCommand cqlCommand);

        public new Batch SetConsistencyLevel(ConsistencyLevel? consistencyLevel)
        {
            base.SetConsistencyLevel(consistencyLevel);
            return this;
        }

        public new Batch SetTimestamp(DateTimeOffset timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        public Batch Append(IEnumerable<CqlCommand> cqlCommands)
        {
            foreach (var cmd in cqlCommands)
            {
                Append(cmd);
            }
            return this;
        }

        public void Execute()
        {
            EndExecute(BeginExecute(null, null));
        }

        protected abstract Task<RowSet> InternalExecuteAsync();
        
        public Task ExecuteAsync()
        {
            return InternalExecuteAsync();
        }

        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            return InternalExecuteAsync().ToApm(callback, state);
        }

        public void EndExecute(IAsyncResult ar)
        {
            var task = (Task)ar;
            task.Wait();
        }
    }
}
