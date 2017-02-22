//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dse.Test.Unit
{
    public class ThreadPerTaskScheduler : TaskScheduler
    {
        protected override IEnumerable<Task> GetScheduledTasks() { return Enumerable.Empty<Task>(); }

        protected override void QueueTask(Task task)
        {
            new Thread(() => TryExecuteTask(task)) { IsBackground = true }.Start();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }
    }
}
