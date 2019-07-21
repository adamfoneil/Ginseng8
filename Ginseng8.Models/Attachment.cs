using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// Blob attachment to an object
    /// </summary>
    public class Attachment : BaseTable, IOrgSpecific
    {
        public Attachment()
        {
        }

        public Attachment(string folderName)
        {
            ObjectType = FromFolderName(folderName);
        }

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        public ObjectType ObjectType { get; set; } = ObjectType.WorkItem;

        public int ObjectId { get; set; }

        /// <summary>
        /// URL with SAS token
        /// </summary>
        [MaxLength(500)]
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// Filename portion of uploaded path
        /// </summary>
        [MaxLength(500)]
        [Required]
        public string DisplayName { get; set; }

        [NotMapped]
        public bool AllowDelete { get; set; }

        public static ObjectType FromFolderName(string folderName)
        {
            try
            {
                Dictionary<string, ObjectType> types = new Dictionary<string, ObjectType>()
                {
                    { "WorkItems", ObjectType.WorkItem },
                    { "Projects", ObjectType.Project },
                    { "DataModels", ObjectType.DataModel },
                    { "ModelClasses", ObjectType.ModelClass },
                    { "Wikis", ObjectType.Article }
                };
                return types[folderName];
            }
            catch (Exception exc)
            {
                throw new Exception($"Couldn't find object type for folder name '{folderName}'.", exc);
            }
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }
}