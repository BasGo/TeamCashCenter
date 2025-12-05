using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeamCashCenter.Data.Model;
using TeamCashCenter.Models;

namespace TeamCashCenter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<AccountController> logger)
        : ControllerBase
    {
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<Role> _roleManager = roleManager;
        private readonly ILogger<AccountController> _logger = logger;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email ?? string.Empty);
            if (user == null)
            {
                return new JsonResult(new LoginResponse { Succeeded = false, ErrorMessage = "Invalid login attempt." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, req.Password ?? string.Empty, req.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded) return new JsonResult(new LoginResponse { Succeeded = true });
            if (result.IsLockedOut) return new JsonResult(new LoginResponse { Succeeded = false, IsLockedOut = true, ErrorMessage = "TargetAccount locked out." });
            return new JsonResult(new LoginResponse { Succeeded = false, ErrorMessage = "Invalid login attempt." });
        }

        [HttpPost("login-form")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoginForm()
        {
            _logger.LogInformation("LoginForm request Content-Type: {ContentType}", Request.ContentType ?? "(none)");

            var req = new LoginRequest();

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                _logger.LogInformation("LoginForm form keys: {Keys}", string.Join(',', form.Keys));
                req.Email = form["Email"].ToString();
                req.Password = form["Password"].ToString();
                req.RememberMe = bool.TryParse(form["RememberMe"].ToString(), out var rm) && rm;
                req.ReturnUrl = form["ReturnUrl"].ToString();
            }
            else
            {
                // try JSON body as fallback
                try
                {
                    Request.EnableBuffering();
                    using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<LoginRequest>(body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (parsed != null) req = parsed;
                        _logger.LogInformation("LoginForm parsed JSON body");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "LoginForm: failed parsing body");
                }
            }

            _logger.LogInformation("LoginForm attempt for {Email}", req.Email);
            var user = await _userManager.FindByEmailAsync(req.Email ?? string.Empty);
            if (user == null)
            {
                _logger.LogWarning("LoginForm: user not found for {Email}", req.Email);
                return Redirect("/login?error=invalid");
            }

            var result = await _signInManager.PasswordSignInAsync(user, req.Password ?? string.Empty, req.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Attempt to log presence of Set-Cookie header (SignInManager should set auth cookie)
                var setCookie = Response.Headers.SetCookie.ToString();
                if (!string.IsNullOrEmpty(setCookie))
                {
                    _logger.LogInformation("LoginForm: sign-in succeeded for {Email}; Set-Cookie headers: {SetCookie}", req.Email, setCookie);
                }
                else
                {
                    _logger.LogInformation("LoginForm: sign-in succeeded for {Email}; no Set-Cookie header found at time of logging.", req.Email);
                }

                var returnUrl = string.IsNullOrEmpty(req.ReturnUrl) ? "/" : req.ReturnUrl;
                return LocalRedirect(returnUrl);
            }

            _logger.LogWarning("LoginForm: sign-in failed for {Email}", req.Email);
            return Redirect("/login?error=invalid");
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new RegisterResponse { Succeeded = false, Errors = modelErrors });
            }

            var user = new User { UserName = req.Email, Email = req.Email };
                var result = await _userManager.CreateAsync(user, req.Password!);
            if (!result.Succeeded)
            {
                var errs = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new RegisterResponse { Succeeded = false, Errors = errs });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new RegisterResponse { Succeeded = true });
        }

        [HttpPost("register-form")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RegisterForm()
        {
            var req = new RegisterRequest();
            string? returnUrl = null;

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                req.Email = form["Email"].ToString();
                req.Password = form["Password"].ToString();
                // optional ReturnUrl
                returnUrl = form["ReturnUrl"].ToString();
            }
            else
            {
                try
                {
                    Request.EnableBuffering();
                    using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<RegisterRequest>(body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (parsed != null) req = parsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RegisterForm: failed parsing body");
                }
            }

            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return Redirect("/account/register?error=invalid");
            }

            var user = new User { UserName = req.Email, Email = req.Email };
            var result = await _userManager.CreateAsync(user, req.Password!);
            if (!result.Succeeded)
            {
                _logger.LogWarning("RegisterForm: create user failed for {Email}: {Errors}", req.Email, string.Join(';', result.Errors.Select(e=>e.Description)));
                return Redirect("/account/register?error=failed");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            var finalReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl!;
            return LocalRedirect(finalReturnUrl);
        }
        
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return Ok(new { Succeeded = true });
        }

        // Form POST friendly logout endpoint so UI can post a regular form and receive a redirect
        [HttpPost]
        [Route("/account/logout")]
        public async Task<IActionResult> LogoutForm()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out via form POST");
            return LocalRedirect("/account/login?loggedout=1");
        }

        // Development-only helper to ensure an admin user exists for local testing.
        [HttpPost("ensure-admin")]
        public async Task<IActionResult> EnsureAdmin()
        {
            try
            {
                var env = HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
                if (env == null || !env.IsDevelopment()) return Forbid();

                var email = "admin@example.com";
                var existing = await _userManager.FindByEmailAsync(email);
                if (existing != null) return Ok(new { Succeeded = true, Message = "Admin exists" });

                var user = new User { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded) return BadRequest(new { Succeeded = false, Errors = result.Errors.Select(e => e.Description) });

                // Ensure the Admin role exists
                var roleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExists)
                {
                    var roleResult = await _roleManager.CreateAsync(new Role("Admin", "Admin"));
                    if (!roleResult.Succeeded)
                    {
                        return StatusCode(500, new { Succeeded = false, Errors = roleResult.Errors.Select(e => e.Description) });
                    }
                }

                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok(new { Succeeded = true, Message = "Admin created" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnsureAdmin failed");
                return StatusCode(500);
            }
        }
    }
}
