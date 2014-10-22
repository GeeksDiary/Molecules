using System;
using Dependable.Dispatcher;

namespace Dependable.Tests
{
    public class Mutation
    {
        public JobStatus? JobStatus { get; set; }

        public int? DispatchCount { get; set; }

        public DateTime? RetryOn { get; set; }

        public Continuation Continuation { get; set; }

        public bool? Suspended { get; set; }
    }
}