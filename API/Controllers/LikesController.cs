using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
      
        private readonly IUnitOfWork _unitOfWork;

        public LikesController(IUnitOfWork unitOfWork)
        {
        
           _unitOfWork = unitOfWork;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId=User.GetUserId();
            var LikedUser= await _unitOfWork.userRepo.GetUserByUsernameAsync(username);
            var SourceUser= await _unitOfWork.likesRepo.GetUserWithLikes(sourceUserId);

            if(LikedUser==null) return NotFound();
            if (SourceUser.UserName == username) return BadRequest("You cannot like YourSelf");
            var likeUser =await _unitOfWork.likesRepo.GetUserLike(sourceUserId, LikedUser.Id);
            if (likeUser != null) return BadRequest("You already like this user");
            var userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = LikedUser.Id
            };
            SourceUser.LikedUsers.Add(userLike);
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("Faild To Like User");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users= await _unitOfWork.likesRepo.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }

    }
}
