namespace Ginseng.Mvc.Classes
{
	public class RoadmapCell
	{
		public RoadmapCell(int appId, int milestoneId)
		{
			ApplicationId = appId;
			MilestoneId = milestoneId;
		}

		public int ApplicationId { get; }
		public int MilestoneId { get; }

		public override bool Equals(object obj)
		{
			var test = obj as RoadmapCell;
			return (test != null) ?
				(test.ApplicationId == ApplicationId && test.MilestoneId == MilestoneId) :
				false;
		}

		public override int GetHashCode()
		{
			return ApplicationId.GetHashCode() + MilestoneId.GetHashCode();
		}
	}
}