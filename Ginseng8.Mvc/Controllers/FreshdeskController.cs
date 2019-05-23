using AutoMapper;
using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Octokit;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Controllers
{
    /// <summary>
    /// thanks to https://gist.github.com/42degrees/0b8876b77005b51dc4bbe391cfa69670
    /// </summary>
    public class FreshdeskController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly DataAccess _data;
        private readonly IFreshdeskService _freshdeskService;

        public FreshdeskController(
            IConfiguration config,
            IMapper mapper,
            IFreshdeskService freshdeskService)
        {
            _config = config;
            _mapper = mapper;
            _data = new DataAccess(_config);
            _freshdeskService = freshdeskService;
        }

        public IActionResult Login(string host_url = null)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Index");

            _data.Initialize(User, TempData);

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

        public ActionResult Logout()
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] WebhookRequest request)
        {
            // check request payload parsed correctly
            if (request == null)
            {
                return BadRequest("Wrong request format");
            }

            // check authentication header
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader))
            {
                return Unauthorized("Authentication header is not provided with the request");
            }

            // validate authentication api key
            var requestApiKey = authHeader.ToString().Replace("Bearer ", "");
            if (!_freshdeskService.ValidateWebhookApiKey(requestApiKey))
            {
                return Unauthorized("Wrong authentication key");
            }

            try
            {
                await _freshdeskService.StoreWebhookPayloadAsync(Request.Body);
            }
            catch
            {
                // ignore, because it's for debug reasons only
            }

            try
            {
                var webhook = _mapper.Map<Webhook>(request);
                await _freshdeskService.OnWebhookAsync(webhook);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return Ok();
        }
    }
}