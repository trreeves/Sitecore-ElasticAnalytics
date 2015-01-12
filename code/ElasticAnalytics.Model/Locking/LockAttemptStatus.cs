using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Model.Locking
{
    /// <summary>
    ///   Describes status of a lock attempt operation.
    /// </summary>
    public enum LockAttemptStatus
    {
        /// <summary>
        ///   Lock has been obtained and the object successfully loaded.
        /// </summary>
        Success = 0,


        /// <summary>
        ///   The object has not been found in the database.
        /// </summary>
        NotFound = 1,


        /// <summary>
        ///   The object is currently locked by another entity. <see cref="LockAttemptResult{T}.LockedBy"/>.
        /// </summary>
        AlreadyLocked = 2,


        /// <summary>
        ///   The database is unavailable.
        /// </summary>
        DatabaseUnavailable = 3,
    }
}
