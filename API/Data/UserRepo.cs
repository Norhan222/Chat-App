using API.Dtos;
using API.Entities;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepo :IUserRepo
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepo(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(u => u.Photos).ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == username);
        }

  
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<pagedList<MemberDto>> GetAllMembersAsync(UserParams userParams)
        {

            var query = _context.Users.AsQueryable();
            query=query.Where(u => u.UserName != userParams.CurrentUser);

            query=query.Where(u=>u.Gender==userParams.Gender);

            var Mindbo = DateTime.Today.AddYears(-userParams.MaxAge -1);
            var Maxdbo=DateTime.Today.AddYears(-userParams.MinAge);

            query=query.Where(u=>u.DateOfBirth >= Mindbo && u.DateOfBirth <=Maxdbo);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await pagedList<MemberDto>.CreateAsync(query
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking(),userParams.PageNumber,userParams.PageSize);
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            var member = await _context.Users.Where(u => u.UserName == username)
             .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
            return member;
        }

        public async Task<string> GetUserGender(string username)
        {
           return await _context.Users
                .Where(u=>u.UserName== username)
                .Select(u=>u.Gender).FirstOrDefaultAsync();
            
        }
    }
}
