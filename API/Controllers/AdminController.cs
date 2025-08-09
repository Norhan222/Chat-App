using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        [Authorize(Policy = "RequieAdminRole")]
        [HttpGet("users-with-roles")]
        public async  Task<ActionResult> GetUsersWithRoles()
        {
            var users= await _userManager.Users
                .Include(u=>u.UserRoles)
                .ThenInclude(r=>r.Role)
                .OrderBy(u=>u.UserName)
                .Select(u =>new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }
        [Authorize(Policy = "RequieAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles) 
        {
            var selectedRoles=roles.Split(",").ToArray();// "mo,admin"

            var user=await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("could not find user");

            var userRoles= await _userManager.GetRolesAsync(user);//"member"

            var result=await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Faild to add roles");

            result=await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Faild to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotoForModeration()
        {
            return Ok("Admin or moderators can see this");
        }
    }
}
