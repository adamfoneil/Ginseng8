namespace Ginseng.Mvc.Classes
{
    public class TeamLabelCell
    {
        public TeamLabelCell(int teamId, int labelId)
        {
            TeamId = teamId;
            LabelId = labelId;
        }

        public int TeamId { get; }
        public int LabelId { get; }

        public override bool Equals(object obj)
        {
            var test = obj as TeamLabelCell;
            return (test != null) ? test.TeamId == TeamId && test.LabelId == LabelId : false;
        }

        public override int GetHashCode()
        {
            return (TeamId + LabelId).GetHashCode();
        }
    }
}