using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Controllers
{
	public class NotifyController : Controller
	{
		private readonly DataAccess _data;
		private readonly Email _email;

		public NotifyController(IConfiguration config)
		{
			_data = new DataAccess(config);
			_email = new Email(config);
		}
	}
}