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
    ///  A reconnection policy that waits a constant time between each reconnection attempt.
    /// </summary>
    public class ConstantReconnectionPolicy : IReconnectionPolicy
    {
        private readonly long _delayMs;

        /// <summary>
        /// Gets the constant delay used by this reconnection policy. 
        /// </summary>
        public long ConstantDelayMs
        {
            get { return _delayMs; }
        }

        /// <summary>
        ///  Creates a reconnection policy that creates with the provided constant wait
        ///  time between reconnection attempts.
        /// </summary>
        /// <param name="constantDelayMs"> the constant delay in milliseconds to use.</param>
        public ConstantReconnectionPolicy(long constantDelayMs)
        {
            if (constantDelayMs > 0)
                _delayMs = constantDelayMs;
            else
                throw new ArgumentException("Constant delay time for reconnection policy have to be bigger than 0.");
        }

        /// <summary>
        ///  A new schedule that uses a constant <c>ConstantDelayMs</c> delay between reconnection attempt. 
        /// </summary>
        /// 
        /// <returns>the newly created schedule.</returns>
        public IReconnectionSchedule NewSchedule()
        {
            return new ConstantSchedule(this);
        }

        private class ConstantSchedule : IReconnectionSchedule
        {
            private readonly ConstantReconnectionPolicy _owner;

            internal ConstantSchedule(ConstantReconnectionPolicy owner)
            {
                _owner = owner;
            }

            public long NextDelayMs()
            {
                return _owner._delayMs;
            }
        }
    }
}
