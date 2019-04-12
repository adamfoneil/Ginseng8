using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class PendingNotifications : Query<Notification>
	{
		public PendingNotifications(int count) : base(
			$@"SELECT TOP ({count}) * FROM [dbo].[Notification] WHERE [DateDelivered] IS NULL {{andWhere}} ORDER BY [DateCreated] ASC")
		{
		}

		[Where("[Method]=@method")]
		public DeliveryMethod? Method { get; set; }
	}
}