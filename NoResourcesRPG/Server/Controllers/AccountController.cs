using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NoResourcesRPG.Server.Database.Models;
using NoResourcesRPG.Shared.Models;

namespace NoResourcesRPG.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AccountController(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, IConfiguration config) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = new UserEntity { Id = Guid.NewGuid().ToString(), DisplayName = req.DisplayName, UserName = req.UserName, Email = req.Email };
        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // Optionally add default role etc.
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = req.Identifier.Contains("@") ?
            await userManager.FindByEmailAsync(req.Identifier) :
            await userManager.FindByNameAsync(req.Identifier);
        if (user == null) return Unauthorized("Invalid credentials");

        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials");

        var sessionTime = TimeSpan.FromDays(1);
        var token = Token.Create(user, config, sessionTime);
        return Ok(new AuthResult(token, sessionTime));
    }
}
