using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
    public class PriorityGroup : AppTable
    {
        [PrimaryKey]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string IconClass { get; set; }

        public bool IsSortable()
        {
            // only the "work on next" items can be prioritized from the All Items page, where priority group is shown
            return (Id == 1);
        }

        public static DataTable GetSeedData()
        {
            return new PriorityGroup[]
            {
                new PriorityGroup() { Name = "Work On Next", Description = "Unassigned items with a priority. Stuff we'd like to work on soon.", IconClass = "far fa-flower-tulip" },
                new PriorityGroup() { Name = "Backlog", Description = "Unassigned items without a priority. Things we don't have time for now.", IconClass = "far fa-flower" },
                new PriorityGroup() { Name = "In Progress", Description = "Assigned items. Use Team page to prioritize.", IconClass = "far fa-bolt" },
                new PriorityGroup() { Name = "Closed", Description = "Closed items don't have priority.", IconClass = "fas fa-check" }
            }.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
        }
    }
}