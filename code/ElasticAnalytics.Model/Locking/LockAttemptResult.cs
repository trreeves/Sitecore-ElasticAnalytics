using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Model.Locking
{
    public class LockAttemptResult<T>
    {
        public LockAttemptResult(LockAttemptStatus status, T lockedObject, ElasticLeaseOwner leaseOwner)
        {
            LockedObject = lockedObject;
            this.Status = status;
            LeaseOwner = leaseOwner;
        }

        public T LockedObject { get; private set; }

        public ElasticLeaseOwner LeaseOwner { get; private set; }

        public LockAttemptStatus Status { get; private set; }
    }
}
