using System.Collections.Generic;

namespace Dependable.Dispatcher
{
    public class JobResult
    {
        public JobResult(Activity activity = null)
        {
            Activity = activity;
        }

        public Activity Activity { get; set; }
    }
}