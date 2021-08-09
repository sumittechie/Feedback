using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ReplyController : ControllerBase
    {
        private readonly FeedbackDbContext _dbContext;

        public ReplyController(FeedbackDbContext feedbackDbContext)
        {
            _dbContext = feedbackDbContext;
        }

        private string CurrentUserId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        }

        [HttpGet]
        public IActionResult GetAssignedFeedbacks()
        {
            var response = new ApiResponse();
            try
            {
                var feedbacks = _dbContext.Reply.Join(_dbContext.Feedback, rply => rply.FeedbackId, fb => fb.FeedbackId, (rply, fb) => new
                {
                    Id = rply.Id,
                    UserId = rply.UsersId,
                    Question = fb.Question,
                    LastUpdated = rply.LastUpdated,
                    CreatedBy = rply.CreatedBy,
                    Reply = rply.Reply
                })
                .Join(_dbContext.Users, rply => rply.CreatedBy, usr => usr.Id, (rply, usr) => new
                {
                    Id = rply.Id,
                    Question = rply.Question,
                    FromUser = usr.Name,
                    LastUpdated = rply.LastUpdated,
                    Reply = rply.Reply,
                    UserId = rply.UserId,
                    Photo = usr.Photo.ToString()

                }).Where(rply => rply.Reply == null && rply.UserId == CurrentUserId())
                .Select(rows => new { Id = rows.Id, Question = rows.Question, From = rows.FromUser, LastUpdated = rows.LastUpdated, Photo = rows.Photo }).ToList();

                response.Error = false;
                response.Data = feedbacks;

            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = ex.Message.ToString();
            }

            return Ok(response);
        }


        [HttpPost]
        public IActionResult PostReply([FromBody] PostReplyVm vm)
        {
            var response = new ApiResponse();
            try
            {
                if (!string.IsNullOrEmpty(vm.Reply))
                {
                    var feedbackAssigned = _dbContext.Reply.FirstOrDefault(r => r.Id == vm.Id);
                    if (feedbackAssigned != null)
                    {
                        feedbackAssigned.Reply = vm.Reply;
                        _dbContext.SaveChanges();
                    }
                    response.Error = false;
                    response.Data = "Feedback reply has been saved successfully";
                }
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = ex.Message.ToString();
            }
            return Ok(response);
        }


    }
}
