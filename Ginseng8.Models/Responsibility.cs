using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
	/// <summary>
	/// Defines the possible values for Activity responsibilities.
	/// In Ginseng there are always and only two responsibilities: Development and Business
	/// </summary>
	public class Responsibility : AppTable
	{
		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		public static DataTable GetSeedData()
		{
			return new Responsibility[]
			{
				new Responsibility() { Name = "Development" },
				new Responsibility() { Name = "Business" }
			}.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
		}
	}
}