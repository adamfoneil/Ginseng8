using Ginseng.Models.Conventions;
using Ginseng.Models.Queries;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// Associates a developer with a milestone in order to determine her/his available hours and feasibility of work items
    /// </summary>
    public class DeveloperMilestone : BaseTable, IFindRelated<int>
    {
        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int DeveloperId { get; set; }

        [References(typeof(Milestone))]
        [PrimaryKey]
        public int MilestoneId { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yy}")]
        public DateTime StartDate { get; set; }

        public Milestone Milestone { get; set; }
        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Milestone = commandProvider.Find<Milestone>(connection, MilestoneId);
        }
        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Milestone = await commandProvider.FindAsync<Milestone>(connection, MilestoneId);
        }

        public override async Task<bool> ValidateAsync(IDbConnection connection)
        {
            var overlap = await new CheckDevMilestoneOverlap() { UserId = DeveloperId, CheckDate = StartDate }.ExecuteAsync(connection);
            
            if (overlap.Any())
            {
                var item = overlap.First();
                ValidateAsyncMessage = $"Can't use this date because it overlaps with another committment: {item.MilestoneName} from {item.StartDate.ToString("M/d/yy")} to {item.EndDate.ToString("M/d/yy")}";
                return false;
            }

            return true;
        }
    }
}
