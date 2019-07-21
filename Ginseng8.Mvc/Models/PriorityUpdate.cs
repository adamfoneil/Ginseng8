namespace Ginseng.Mvc.Models
{
    public class PriorityUpdate
    {
        /// <summary>
        /// Milestone that item was dragged onto
        /// </summary>
        public int MilestoneId { get; set; }

        /// <summary>
        /// User that item was dragged onto
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Work item that was dragged
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Field being updated when work item dragged across custom grouping
        /// </summary>
        public string GroupFieldName { get; set; }

        /// <summary>
        /// Value to set when work item dropped in this group
        /// </summary>
        public int GroupFieldValue { get; set; }

        /// <summary>
        /// List of items in the milestone + user combination drop area after drag-dropping
        /// </summary>
        public Item[] Items { get; set; }
    }

    public class ProjectPriorityUpdate
    {
        public Item[] Items { get; set; }
    }

    public class Item
    {
        /// <summary>
        /// Work item number (or Project.Id)
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Position after drag-drop
        /// </summary>
        public int Index { get; set; }
    }
}