using API.Dtos;
using API.Entities;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepo : IMessageRepo
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepo(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnectionAsync(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(c => c.Connections)
                  .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                  .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessageAsync(int id)
        {
            return await _context.Messages
                .Include(m=>m.Sender)
                .Include(m=>m.Recipient)
                .SingleOrDefaultAsync(m=>m.Id==id);
        }

        public async Task<pagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox"=> query.Where(u=>u.RecipientUserName ==messageParams.UserName 
                         &&u.RecipientDeleted ==false),
                "Outbox"=>query.Where(u=>u.SenderUserName==messageParams.UserName &&u.SenderDeleted==false),
                _=>query.Where(u => u.RecipientUserName == messageParams.UserName && 
                         u.RecipientDeleted==false&&u.DateRead ==null)

            };
           
            return await pagedList<MessageDto>.CreateAsync(query,messageParams.PageNumber
                ,messageParams.PageSize);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName,
            string recipientUserName)
        {
            var messages = await _context.Messages
         
                .Where(m => m.Recipient.UserName == currentUserName &&m.RecipientDeleted==false
                && m.Sender.UserName == recipientUserName
                || m.Recipient.UserName == recipientUserName
                && m.Sender.UserName == currentUserName &&m.SenderDeleted==false
                )
                .OrderBy(m => m.MessageSent)
                //.ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var UnredaMessages=messages.Where(m=>m.DateRead==null 
               &&m.RecipientUserName==currentUserName).ToList();
            if (UnredaMessages.Any())
            {
                foreach (var message in UnredaMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }
            return _mapper.Map<IEnumerable <MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}
