using Ginseng.Models.Interfaces;
using Ginseng.Models.Queries;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Models.Conventions
{
    [Identity(nameof(Id), IdentityPosition.FirstColumn)]
    public abstract class BaseTable : Record
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        [ColumnAccess(SaveAction.Insert)]
        public string CreatedBy { get; set; }

        [ColumnAccess(SaveAction.Insert)]
        public DateTime DateCreated { get; set; }

        [MaxLength(50)]
        [ColumnAccess(SaveAction.Update)]
        public string ModifiedBy { get; set; }

        [ColumnAccess(SaveAction.Update)]
        public DateTime? DateModified { get; set; }

        public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
        {
            // this is okay during event logging because the user has already been captured.
            // BeforeSave handlers run during EventLog.LogAsync, but we need to return if no user is set or we get null ref down below
            if (user == null) return;

            switch (action)
            {
                case SaveAction.Insert:
                    CreatedBy = user.UserName;
                    DateCreated = user.LocalTime;
                    break;

                case SaveAction.Update:
                    ModifiedBy = user.UserName;
                    DateModified = user.LocalTime;
                    break;
            }
        }

        public override async Task<bool> CheckSavePermissionAsync(IDbConnection connection, IUser user)
        {
            return await CheckOrgPermissionAsync(connection, user);
        }

        public override async Task<bool> CheckFindPermissionAsync(IDbConnection connection, IUser user)
        {
            return await CheckOrgPermissionAsync(connection, user);
        }

        private async Task<bool> CheckOrgPermissionAsync(IDbConnection connection, IUser user)
        {
            var orgSpecific = this as IOrgSpecific;
            if (orgSpecific != null)
            {
                var userOrgIds = await GetUserOrgIdsAsync(connection, user);
                int thisOrgId = await orgSpecific.GetOrgIdAsync(connection);
                return userOrgIds.Contains(thisOrgId);
            }

            return await base.CheckFindPermissionAsync(connection, user);
        }

        private async Task<int[]> GetUserOrgIdsAsync(IDbConnection connection, IUser user)
        {
            var result = await new OrgUserByName() { UserName = user.UserName }.ExecuteAsync(connection);
            return result.Select(row => row.OrganizationId).ToArray();
        }
    }
}