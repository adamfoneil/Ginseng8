namespace Ginseng.Mvc.Classes
{
    public class AppLabelCell
    {
        public AppLabelCell(int applicationId, int labelId)
        {
            ApplicationId = applicationId;
            LabelId = labelId;
        }

        public int ApplicationId { get; set; }
        public int LabelId { get; set; }

        public override bool Equals(object obj)
        {
            var test = obj as AppLabelCell;
            return (test != null) ? test.ApplicationId == ApplicationId && test.LabelId == LabelId : false;
        }

        public override int GetHashCode()
        {
            return (ApplicationId + LabelId).GetHashCode();
        }
    }
}