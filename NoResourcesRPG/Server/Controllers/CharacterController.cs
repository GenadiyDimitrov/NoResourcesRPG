using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoResourcesRPG.Server.Database;
using NoResourcesRPG.Server.Database.Models;
using NoResourcesRPG.Shared.Models;
using NoResourcesRPG.Shared.Static;
using System.Drawing;

namespace NoResourcesRPG.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CharacterController : ControllerBase
{
    private readonly NoResDbContext _db;
    private readonly UserManager<UserEntity> _userManager;

    public CharacterController(NoResDbContext db, UserManager<UserEntity> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyCharacters()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var chars = await _db.Characters
            .Where(c => c.UserId == user.Id)
            .Select(c => c.Adapt<Character>())
            .ToListAsync();

        return Ok(chars);
    }

    [HttpPost("my/create")]
    public async Task<IActionResult> CreateNewCharacter([FromBody] string name)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        if (await _db.Characters.AsNoTracking().Where(x => x.UserId == user.Id).CountAsync() >= 8)
        {
            return BadRequest("Cannot exceed 8 characters");
        }
        if (await _db.Characters.AsNoTracking().AnyAsync(c => c.Name == name))
        {
            return BadRequest("Character name already exists");
        }
        await _db.Characters.AddAsync(new CharacterEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Color = ColorExtensions.RandomHexColor,
            Level = 1,
            X = 0,
            Y = 0,
            ViewRange = 5,
            UserId = user.Id
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("my/delete/{id}")]
    public async Task<IActionResult> DeleteCharacter(string id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var entity = await _db.Characters.FindAsync(id);
        if (entity == null || entity.UserId != user.Id)
        {
            // invalid character or not owned by user
            return Unauthorized();
        }
        _db.Characters.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
