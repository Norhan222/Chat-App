using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helper;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Buffers.Text;

namespace API.Controllers
{
   
    public class MessagesController : BaseApiController
    {
      
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork,IMapper mapper)
        {
           
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

      
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            messageParams.UserName = User.GetUsername();
            var messages=await _unitOfWork.messageRepo.GetMessageForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.Count, messages.TotalPages);
            return Ok(messages);
        }
     
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username=User.GetUsername();
            var message=await _unitOfWork.messageRepo.GetMessageAsync(id);
            if (message.Sender.UserName!=username &&message.Recipient.UserName !=username)
                return Unauthorized();

            if(message.Sender.UserName==username) message.SenderDeleted = true;

            if(message.Recipient.UserName==username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.messageRepo.DeleteMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("problem deleting the message");
        }
    }
}
