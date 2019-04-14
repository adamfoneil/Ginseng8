namespace Ginseng.Mvc.Models
{
	public class DefaultActivity
	{
		/// <summary>
		/// Base on OrganizationUser.Responsibilities (column name itself comes from Responsibility.WorkItemColumnName
		/// </summary>
		public string UserIdColumn { get; set; }

		/// <summary>
		/// From OrganizationUser.DefaultActivityId
		/// </summary>
		public int ActivityId { get; set; }
	}
}