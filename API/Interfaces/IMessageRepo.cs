using API.Dtos;
using API.Entities;
using API.Helper;

namespace API.Interfaces
{
    public interface IMessageRepo
    {
        void AddGroup(Group group);

        void RemoveConnection(Connection connection);
        Task<Connection> GetConnectionAsync(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupForConnection(string connectionId);
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessageAsync(int id);
        Task<pagedList<MessageDto>> GetMessageForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName);


    }
}
