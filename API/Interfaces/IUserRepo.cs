using API.Dtos;
using API.Entities;
using API.Helper;

namespace API.Interfaces
{
    public interface IUserRepo
    {
        void Update(AppUser user);
    
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<pagedList<MemberDto>> GetAllMembersAsync(UserParams userParams);
        Task<MemberDto> GetMemberAsync(string username);
        Task<string> GetUserGender(string username);
    }
}
