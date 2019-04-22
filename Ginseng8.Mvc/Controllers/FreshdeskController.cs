using Ginseng.Models;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ginseng.Mvc.Controllers
{
	/// <summary>
	/// thanks to https://gist.github.com/42degrees/0b8876b77005b51dc4bbe391cfa69670
	/// </summary>
	public class FreshdeskController : Controller
	{
		private readonly IConfiguration _config;
		private readonly DataAccess _data;

		public FreshdeskController(IConfiguration config)
		{
			_config = config;
			_data = new DataAccess(_config);
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);
			_data.Initialize(User, TempData);
		}

		public IActionResult Login(string host_url = null)
		{
			if (!User.Identity.IsAuthenticated) return RedirectToPage("/Index");

			string secret = _config["Freshdesk:SSOSecret"];
			string portalHost = (!string.IsNullOrEmpty(host_url)) ? host_url : _config["Freshdesk:HostUrl"];

			var id = User.Identity as ClaimsIdentity;
			var userNameTrimmed = id.Name.Trim();
			UserProfile user = _data.CurrentUser;
			OrganizationUser orgUser = _data.CurrentOrgUser;
			var name = orgUser.DisplayName ?? user.Email;
			var email = user.Email;
			var timems = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString();

			// Get a hash signature that proves the request originated from a registered customer.
			var hash = GetFreshdeskHash(secret, name, email, timems);

			var path = $@"https://{portalHost}/login/sso?name={WebUtility.UrlEncode(name)}&email={WebUtility.UrlEncode(email)}&timestamp={timems}&hash={hash}";

			return Redirect(path);
		}

		public ActionResult FreshdeskLogout()
		{
			return View();
		}

		/// <summary>
		/// Special hash required by FreshDesk to validate that the sender is an authorized user.
		/// The parameters in the hash must also be available in the URL that is generated, plus
		/// the one secret value assigned to a specific FreshDesk license.
		/// </summary>
		/// <param name="secret">A secret value (probably a GUID) provided by FreshDesk</param>
		/// <param name="name">The name of the person logging in (for display).</param>
		/// <param name="email">The e-mail address of the person logging in (for contact).</param>
		/// <param name="timems">A time value that makes the block unique even for the same user.</param>
		/// <returns>A hash "signature" for this login.</returns>
		private static string GetFreshdeskHash(string secret, string name, string email, string timems)
		{
			var input = name + secret + email + timems;
			var keybytes = Encoding.Default.GetBytes(secret);
			var inputBytes = Encoding.Default.GetBytes(input);

			var crypto = new HMACMD5(keybytes);
			var hash = crypto.ComputeHash(inputBytes);

			return hash.Select(b => b.ToString("x2"))
					   .Aggregate(new StringBuilder(),
								  (current, next) => current.Append(next),
								  current => current.ToString());
		}
	}
}