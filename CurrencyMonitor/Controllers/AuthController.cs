using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyMonitor.Controllers
{
    public class AuthController : Controller
    {
		private readonly IAuthenticationSchemeProvider authenticationSchemeProvider;

		public AuthController(IAuthenticationSchemeProvider authenticationSchemeProvider)
		{
			this.authenticationSchemeProvider = authenticationSchemeProvider;
		}

		public async Task<IActionResult> Login()
		{
			return View(from scheme in (await authenticationSchemeProvider.GetAllSchemesAsync())
						where !string.IsNullOrEmpty(scheme.DisplayName)
						select scheme.DisplayName);
		}

		public IActionResult SignIn(string provider)
		{
			return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);
		}

		public async Task<IActionResult> SignOut()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Home");
		}

	}// end of class AuthController

}// end of namespace CurrencyMonitor.Controllers
