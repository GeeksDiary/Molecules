using System;
using System.Collections.Generic;
using System.Linq;

namespace Dependable.Dispatcher
{
    public class Continuation
    {
        public Continuation()
        {
            Children = Enumerable.Empty<Continuation>();
        }

        public Guid Id { get; set; }

        public ContinuationType Type { get; set; }

        public JobStatus Status { get; set; }

        public IEnumerable<Continuation> Children{ get; set; }

        public Continuation Next { get; set; }

        public Continuation OnAnyFailed { get; set; }

        public Continuation OnAllFailed { get; set; }

        public bool ContinueAfterHandlingFailure { get; set; }
    }
}