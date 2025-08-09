using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UnitOfWork(DataContext context,IMapper mapper)
        {
           _context = context;
            _mapper = mapper;
        }
        public IUserRepo userRepo => new UserRepo(_context,_mapper);

        public IMessageRepo messageRepo => new MessageRepo(_context, _mapper);

        public ILikesRepo likesRepo => new LikesRepo(_context);

    

        public async Task<bool> Complete()
        {
          return await _context.SaveChangesAsync() >0;
        }

        public bool HasChanges()
        {
           return _context.ChangeTracker.HasChanges();
        }
    }
}
