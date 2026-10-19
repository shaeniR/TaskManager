using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Exceptions;
using TaskManager.API.Extensions;
using TaskManager.API.Models;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    // Used by the "Add member" picker
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> Search([FromQuery] string? search)
    {
        var query = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

        var users = await query.OrderBy(u => u.Name).Take(20).ToListAsync();
        return Ok(users.Select(u => u.ToDto()));
    }

    public record ChangeRoleDto(string Role);

    // Role-based authorization: only Admins can promote/demote users
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<UserDto>> ChangeRole(int id, ChangeRoleDto dto)
    {
        if (dto.Role != Roles.Admin && dto.Role != Roles.Member)
            throw new BadRequestException("Role must be 'Admin' or 'Member'.");
        if (id == User.GetUserId())
            throw new BadRequestException("You cannot change your own role.");

        var user = await _db.Users.FindAsync(id) ?? throw new NotFoundException("User not found.");
        user.Role = dto.Role;
        await _db.SaveChangesAsync();
        return Ok(user.ToDto());
    }
}
