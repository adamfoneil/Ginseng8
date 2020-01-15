using Dapper.QX;
using Ginseng.Models.Queries;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Base.Interfaces;
using System.Data.SqlClient;

namespace Testing
{
    [TestClass]
    public class QueryTests
    {
        // helpful sources:
        // https://www.jerriepelser.com/blog/using-configuration-files-in-dotnet-core-unit-tests/
        // https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects

		private readonly IConfiguration _config;

        public QueryTests()
        {
            _config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
        }

        private SqlConnection GetConnection()
        {
            string connectionStr = _config.GetSection("ConnectionStrings").GetValue<string>("Default");
            return new SqlConnection(connectionStr);
        }

        [TestMethod]
        public void WorkItemsQuery()
        {
            QueryHelper.Test<OpenWorkItems>(GetConnection);
        }

        [TestMethod]
        public void EventLogsQuery()
        {
            QueryHelper.Test<EventLogs>(GetConnection);
        }

        [TestMethod]
        public void EventSubscriptionEmailNotifications()
        {
            QueryHelper.Test<InsertEventSubscriptionEmailNotifications>(GetConnection);           
        }

        [TestMethod]
        public void EventSubscriptionTextNotifications()
        {
            QueryHelper.Test<InsertEventSubscriptionTextNotifications>(GetConnection);
        }

        [TestMethod]
        public void ActivitySubscriptionEmailNotifications()
        {
            QueryHelper.Test<InsertActivitySubscriptionEmailNotifications>(GetConnection);
        }

        [TestMethod]
        public void ActivitySubscriptionTextNotifications()
        {
            QueryHelper.Test<InsertActivitySubscriptionTextNotifications>(GetConnection);
        }

        [TestMethod]
        public void ProjectInfoQuery()
        {
            QueryHelper.Test<ProjectInfo>(GetConnection);
        }

        [TestMethod]
        public void PendingNotificationsQuery()
        {
            QueryHelper.Test<PendingNotifications>(GetConnection);
        }

        [TestMethod]
        public void OrgUserByNameQuery()
        {
            QueryHelper.Test<OrgUserByName>(GetConnection);
        }

        [TestMethod]
        public void NextPriorityQuery()
        {
            QueryHelper.Test<NextPriority>(GetConnection);
        }

        [TestMethod]
        public void AllPendingWorkLogsQuery()
        {
            QueryHelper.Test<AllPendingWorkLogs>(GetConnection);
        }

        [TestMethod]
        public void AllInvoiceWorkItemsQuery()
        {
            QueryHelper.Test<AllInvoiceWorkLogs>(GetConnection);
        }

        [TestMethod]
        public void CalendarWeeksQuery()
        {
            QueryHelper.Test<CalendarWeeks>(GetConnection);
        }

        [TestMethod]
        public void LabelsQuery()
        {
            QueryHelper.Test<Labels>(GetConnection);
        }

        [TestMethod]
        public void MyWorkScheduleQuery()
        {
            QueryHelper.Test<MyWorkSchedule>(GetConnection);
        }

        [TestMethod]
        public void MilestoneMetricsQuery()
        {
            QueryHelper.Test<MilestoneMetrics>(GetConnection);
        }

        [TestMethod]
        public void InsertDefaultLabelSubscriptionsQuery()
        {
            QueryHelper.Test<InsertDefaultLabelSubscriptions>(GetConnection);
        }

        [TestMethod]
        public void MilestonesQuery()
        {
            QueryHelper.Test<Milestones>(GetConnection);
        }

        [TestMethod]
        public void MyOrgUsersQuery()
        {
            QueryHelper.Test<MyOrgUsers>(GetConnection);
        }

        [TestMethod]
        public void DevMilestoneWorkingHoursQuery()
        {
            QueryHelper.Test<DevMilestoneWorkingHours>(GetConnection);
        }

        [TestMethod]
        public void TeamsQuery()
        {
            QueryHelper.Test<Teams>(GetConnection);
        }

        [TestMethod]
        public void LabelSubscriptionUsersQuery()
        {
            QueryHelper.Test<LabelSubscriptionUsers>(GetConnection);
        }

        [TestMethod]
        public void CalendarProjectsQuery()
        {
            QueryHelper.Test<DevCalendarProjects>(GetConnection);
        }

        [TestMethod]
        public void MyOptionsQuery()
        {
            QueryHelper.Test<MyOptions>(GetConnection);
        }

        [TestMethod]
        public void MyActivityOrderQuery()
        {
            QueryHelper.Test<MyActivityOrder>(GetConnection);
        }

        [TestMethod]
        public void AllMilestonesQuery()
        {
            QueryHelper.Test<AllMilestones>(GetConnection);
        }

        [TestMethod]
        public void HiddenMilestonesQuery()
        {
            QueryHelper.Test<HiddenMilestones>(GetConnection);
        }

        [TestMethod]
        public void NewItemAppLabelsQuery()
        {
            QueryHelper.Test<NewItemAppLabels>(GetConnection);
        }

        [TestMethod]
        public void TeamLabelsQuery()
        {
            QueryHelper.Test<TeamLabels>(GetConnection);
        }

        [TestMethod]
        public void ItemCountsByLabelQuery()
        {
            QueryHelper.Test<ItemCountsByLabel>(GetConnection);
        }
    }
}