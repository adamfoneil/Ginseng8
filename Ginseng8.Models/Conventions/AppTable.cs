using Postulate.Base.Attributes;

namespace Ginseng.Models.Conventions
{
	/// <summary>
	/// App tables have data owned by the application,
	/// and are read-only to users
	/// </summary>
	[Schema("app")]
	[Identity(nameof(Id), IdentityPosition.FirstColumn)]
	public abstract class AppTable
	{
		public int Id { get; set; }
	}
}