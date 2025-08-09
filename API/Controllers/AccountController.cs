using API.Data;
using API.Dtos;
using API.Entities;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,ITokenService tokenService,IMapper mapper)
        {
           _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
           _mapper = mapper;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName))
                return BadRequest("UserName Is Taken");
            //registerDto.DateOfBirth.ToString("o", CultureInfo.InvariantCulture);
            
            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.UserName.ToLower();


            var result=await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var resultRole = await _userManager.AddToRoleAsync(user, "Member");
            if (!resultRole.Succeeded) return BadRequest(resultRole.Errors);
            return Ok(new UserDto
            {
                Username = user.UserName,
                Token =await  _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender=user.Gender
            });

        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
           
            var user = await _userManager.Users.
                Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());
            if (user is null)
                return Unauthorized("Invalid username");
            var result=await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,false);

            if (!result.Succeeded) return Unauthorized();
            return Ok(new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender=user.Gender
                
            });
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
        }
    }
}
