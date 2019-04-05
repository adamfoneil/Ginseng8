using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
	public class TimeZone : AppTable
	{
		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		/// <summary>
		/// Values less than 24 are treated as hours, greater than treated as minutes.
		/// (This is how hour-fractional time zones are implemented such as India Standard Time)		
		/// </summary>
		[UniqueKey]
		public int Offset { get; set; }

		public static DataTable GetSeedData()
		{
			return new TimeZone[]
			{
				new TimeZone() { Name = "East US", Offset = -5 },
				new TimeZone() { Name = "India Standard Time", Offset = 330 }, // 5.5 hrs, for Kiran should he ever work on this again
				new TimeZone() { Name = "Central US", Offset = -6 },
				new TimeZone() { Name = "East Euro", Offset = 2 }, // for Alexey
				new TimeZone() { Name = "Mountain US", Offset = -7 },
				new TimeZone() { Name = "Pacific US", Offset = -8 }
			}.ToDataTable(new SqlServerIntegrator(), excludeIdentity:true);
		}
	}
}