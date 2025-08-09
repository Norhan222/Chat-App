using API.Data;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub:Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly PresenceTracker _tracker;
        private readonly IHubContext<PresenceHub> _presence;

        public MessageHub(IUnitOfWork unitOfWork,IMapper mapper
            ,PresenceTracker tracker, IHubContext<PresenceHub> presence)
        {
           _unitOfWork = unitOfWork;
            _mapper = mapper;
          
            _tracker = tracker;
            _presence = presence;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext=Context.GetHttpContext();

            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName=GetGroupName(Context.User.GetUsername(),otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group= await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages =await  _unitOfWork.messageRepo
                .GetMessageThread(Context.User.GetUsername(), otherUser);
            if( _unitOfWork.HasChanges()) await _unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread",messages);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group=await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }


        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            if (username == createMessageDto.RecipientUserName.ToLower())
                throw new HubException("You cannot message your self");
            var sender = await _unitOfWork.userRepo.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.userRepo.GetUserByUsernameAsync(createMessageDto.RecipientUserName);
            if (recipient is null) throw new HubException("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientUserName = recipient.UserName,
                content = createMessageDto.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group= await _unitOfWork.messageRepo.GetMessageGroup(groupName);
            if (group.Connections.Any(x => x.UserName == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if(connections is not null)
                {
                    await _presence.Clients.Clients(connections).SendAsync("NewMessageReceiver", new
                    {
                        username=sender.UserName,
                        knownAs=sender.KnownAs
                    });
                }
            }

            _unitOfWork.messageRepo.AddMessage(message);
            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }


        public async Task<Group> AddToGroup(string groupName)
        {
            var group=await _unitOfWork.messageRepo.GetMessageGroup(groupName);
            var connection= new Connection(Context.ConnectionId,Context.User.GetUsername());
            if(group is null)
            {
                group =new Group(groupName);
                _unitOfWork.messageRepo.AddGroup(group);
            }
            group.Connections.Add(connection);
            if (await _unitOfWork.Complete())return group;
            throw new HubException("Faild to join group");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            var group= await _unitOfWork.messageRepo.GetGroupForConnection(Context.ConnectionId);
            var connection=group.Connections.FirstOrDefault(x=>x.ConnectionId==Context.ConnectionId);
            _unitOfWork.messageRepo.RemoveConnection(connection);
           if( await _unitOfWork.Complete())return group;
            throw new HubException("Faild to remove hub group");
        }
        private string GetGroupName(string caller,string othor)
        {
            var stringCompare=string.CompareOrdinal(caller, othor) <0;
            return stringCompare ?$"{caller}-{othor}":$"{othor}-{caller}";
        }

    }
}
