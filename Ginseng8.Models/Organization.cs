using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class Organization : BaseTable, IFindRelated<int>, IOrgSpecific
    {
        [PrimaryKey]
        [MaxLength(50)]
        public string Name { get; set; }

        [References(typeof(UserProfile))]
        [ColumnAccess(SaveAction.Insert)]
        public int OwnerUserId { get; set; }

        [DefaultExpression("1000")]
        public int NextWorkItemNumber { get; set; } = 1000;

        [DefaultExpression("1000")]
        public int NextInvoiceNumber { get; set; } = 1000;

        [DefaultExpression("3")]
        public int IterationWeeks { get; set; } = 3;

        [DefaultExpression("6")]
        public int MilestoneWorkDayValue { get; set; } = 6; // Friday

        /// <summary>
        /// Default developer activity for users who haven't set it
        /// </summary>
        [References(typeof(Activity))]
        public int? DeveloperActivityId { get; set; }

        /// <summary>
        /// Used with Microsoft OAuth at least, maybe others
        /// </summary>
        [MaxLength(50)]
        [Required]
        [UniqueKey]
        public string TenantName { get; set; }

        [MaxLength(255)]
        public string FreshdeskUrl { get; set; }

        [MaxLength(255)]
        public string FreshdeskApiKey { get; set; }

        [MaxLength(500)]
        public string InvoiceHeader { get; set; }

        [MaxLength(255)]
        public string PaymentLink { get; set; }

        public WorkDay MilestoneWorkDay { get; set; }
        public UserProfile OwnerUser { get; set; }

        public override bool Validate(IDbConnection connection, out string message)
        {
            if (Name.Contains("--"))
            {
                message = "Name may not contain consecutive dashes.";
                return false;
            }

            var allowedChars = "abcdefghijklmnopqrstuvwxyz1234567890-".ToCharArray();
            var nameChars = Name.Select(c => char.ToLower(c)).ToArray();
            var invalid = nameChars.Except(allowedChars);

            if (invalid.Any())
            {
                message = "Organization name may contain letters, numbers, and dashes only.";
                return false;
            }

            if (IterationWeeks < 0)
            {
                message = "Iteration Weeks cannot be less than one.";
                return false;
            }

            message = null;
            return true;
        }

        public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
        {
            base.BeforeSave(connection, action, user);

            if (action == SaveAction.Insert)
            {
                UserProfile profile = user as UserProfile;
                if (profile != null) OwnerUserId = profile.UserId;
            }
        }

        public override void AfterSave(IDbConnection connection, SaveAction action, IUser user)
        {
            base.AfterSave(connection, action, user);

            if (action == SaveAction.Insert)
            {
                var profile = connection.Find<UserProfile>(OwnerUserId);
                if (!profile.OrganizationId.HasValue)
                {
                    profile.OrganizationId = Id;
                    connection.Update(profile, null, r => r.OrganizationId);
                }
            }
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            MilestoneWorkDay = commandProvider.FindWhere<WorkDay>(connection, new { Value = MilestoneWorkDayValue });
            OwnerUser = commandProvider.Find<UserProfile>(connection, OwnerUserId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            MilestoneWorkDay = await commandProvider.FindAsync<WorkDay>(connection, MilestoneWorkDayValue);
            OwnerUser = await commandProvider.FindAsync<UserProfile>(connection, OwnerUserId);
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(Id);
        }

        public bool UseFreshdesk()
        {
            return !string.IsNullOrEmpty(FreshdeskUrl) && !string.IsNullOrEmpty(FreshdeskApiKey);
        }
    }
}