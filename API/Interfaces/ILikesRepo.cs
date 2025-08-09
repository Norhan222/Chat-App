using API.Dtos;
using API.Entities;
using API.Helper;

namespace API.Interfaces
{
    public interface ILikesRepo
    {
        Task<UserLike> GetUserLike(int soucrceUserId, int likedUserid);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<pagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}
