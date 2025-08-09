using API.Data;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUnitOfWork unitOfWork,IMapper mapper,IPhotoService photoService)
        {
          
            
           _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {

            var gender = await _unitOfWork.userRepo.GetUserGender(User.GetUsername());
            userParams.CurrentUser = User.GetUsername();
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender =gender != "male" ? "female" : "male";
            }
            var users = await _unitOfWork.userRepo.GetAllMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<AppUser>> GetUser(string username)
        {
            var user = await _unitOfWork.userRepo.GetMemberAsync(username);
            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto model)
        {
            var user = await _unitOfWork.userRepo.GetUserByUsernameAsync(User.GetUsername());
            var mappedUser = _mapper.Map(model, user);
            _unitOfWork.userRepo.Update(mappedUser);
            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("Faild To Update User");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.userRepo.GetUserByUsernameAsync(User.GetUsername());
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
                return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUri.AbsoluteUri,
                PublicId = result.PublicId,
            };
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await _unitOfWork.Complete())
            {
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.userRepo.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo.IsMain) return BadRequest("this is already main photo");
            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Faild to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.userRepo.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You Cannot delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete())
                return Ok();
            return BadRequest("Faild To delete photo");

        }
    }
}
