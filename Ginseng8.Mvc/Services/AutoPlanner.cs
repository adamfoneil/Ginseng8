using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Postulate.SqlServer.IntKey;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
    /// <summary>
    /// Sets WorkItem.Dates according to your estimates and work hours
    /// </summary>
    public class AutoPlanner
    {        
        public async Task ExecuteAsync(SqlConnection connection, int orgId, int userId, int milestoneId, DateTime startDate)
        {
            var ms = await connection.FindAsync<Milestone>(milestoneId);
            var workHours = await new DailyWorkHours() { OrgId = orgId, UserId = userId, StartDate = startDate, EndDate = ms.Date }.ExecuteAsync(connection);
            var itemEstimates = await new WorkItemEstimateHours() { OrgId = orgId, UserId = userId, MilestoneId = milestoneId }.ExecuteAsync(connection);

            foreach (var item in itemEstimates)
            {                
                foreach (var day in workHours)
                {
                    
                }
                
            }
        }
    }
}