using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepo : ILikesRepo
    {
        private readonly DataContext _context;

        public LikesRepo(DataContext context)
        {
           _context = context;
        }
        public async Task<UserLike> GetUserLike(int soucrceUserId, int likedUserid)
        {
            return await _context.Likes.FindAsync(soucrceUserId, likedUserid);
        }

        public async Task<pagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users=_context.Users.OrderBy(u=>u.UserName).AsQueryable();
            var likes=_context.Likes.AsQueryable();
            if (likesParams.Predicate == "liked")
            {
                likes=likes.Where(like=>like.SourceUserId ==likesParams.UserId);
                users=likes.Select(like=>like.LikedUser);
            }
            if(likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }
            var likedUser = users.Select(u => new LikeDto
            {
                Id = u.Id,
                UserName = u.UserName,
                KnownAs = u.KnownAs,
                Age = u.DateOfBirth.CalculateAge(),
                City = u.City,
                PhotoUrl = u.Photos.FirstOrDefault(p => p.IsMain).Url

            }).AsQueryable();
            return await pagedList<LikeDto>.CreateAsync(likedUser,likesParams.PageNumber,likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(u => u.LikedUsers)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
