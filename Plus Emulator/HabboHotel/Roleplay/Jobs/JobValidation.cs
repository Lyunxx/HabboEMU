using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Jobs
{
    static class JobValidation
    {
        /// <summary>
        /// Validates a job rank using the suggested job id and rank id.
        /// </summary>
        /// <param name="JobId">JobId of the job.</param>
        /// <param name="RankId">RankId of the job.</param>
        /// <returns>True if parsed or false if not.</returns>
        public static bool ValidateJob(int JobId, int RankId)
        {
            // I realize.
            return JobManager.validJob(JobId, RankId) ? true : false;
        }
    }
}
