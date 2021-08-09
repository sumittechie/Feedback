using Data;
using Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Models.ViewModels;
using System.Security.Claims;
using System.Security.Authentication;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FeedbackController : ControllerBase
    {

        private readonly FeedbackDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackController(FeedbackDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var feedbacks = _dbContext.Feedback.Join(_dbContext.Users,
                    fb => fb.CreatedBy,
                    us => us.Id,
                    (fb, us) => new { fb.FeedbackId, fb.Question, CreatedBy = us.Name, fb.LastUpdated });

                return Ok(new ApiResponse { Error = false, Data = feedbacks });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { Error = true, Message = ex.Message.ToString() });
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var feedback = _dbContext.Feedback.FirstOrDefault(row => row.FeedbackId == id);
                var assignee = _dbContext.Reply.Join(_dbContext.Users,
                    fba => fba.UsersId,
                    us => us.Id,
                    (fba, us) => new { feedbackId = fba.FeedbackId, id = fba.UsersId, name = $"{us.Name} [{us.Email}]" })
                    .Where(fba => fba.feedbackId == id).ToList();
                var result = new
                {
                    Feedback = feedback,
                    Assignee = assignee
                };

                return Ok(new ApiResponse { Error = false, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { Error = true, Message = ex.Message.ToString() });
            }
        }

        [HttpPost]
        public IActionResult Post(string feedback)
        {
            try
            {
                Feedback obj = new Feedback { Question = feedback.Trim(), CreatedBy = GetUserId(), LastUpdated = DateTime.Now };

                _dbContext.Feedback.Add(obj);
                _dbContext.SaveChanges();
                return Ok(new ApiResponse { Error = false, Data = "Feedback created successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { Error = true, Message = ex.Message.ToString() });
            }

        }


        [HttpPost]
        [Route("SaveFeedback")]
        public IActionResult SaveFeedback([FromBody] FeedbackVm model)
        {
            try
            {
                var currentUserId = GetUserId();
                if (model.FeedbackId.Value > 0)
                {
                    var feedbackObj = _dbContext.Feedback.FirstOrDefault(fb => fb.FeedbackId == model.FeedbackId);
                    if (feedbackObj != null)
                    {

                        feedbackObj.Question = model.Question;
                        _dbContext.SaveChanges();

                        // Updating assinees
                        var existingAssignees = _dbContext.Reply.Where(fba => fba.FeedbackId == model.FeedbackId).Select(fba => fba.UsersId).ToList();

                        //Users need to removed from feedback
                        var removeAssignee = existingAssignees.Except(model.Users).ToList();
                        if (removeAssignee.Count > 0)
                        {
                            foreach (var user in removeAssignee)
                            {
                                var removeAssigneeObj = _dbContext.Reply.Where(fba => fba.UsersId == user && fba.FeedbackId == model.FeedbackId.Value).FirstOrDefault();
                                _dbContext.Reply.Remove(removeAssigneeObj);
                            }
                        }

                        var addAssignee = model.Users.Except(existingAssignees).ToList();

                        if (addAssignee.Count > 0)
                        {
                            foreach (var user in addAssignee)
                            {
                                var ReplyObj = new Replies { FeedbackId = model.FeedbackId.Value, UsersId = user, CreatedBy = currentUserId, LastUpdated = DateTime.Now };
                                _dbContext.Reply.Add(ReplyObj);
                            }
                        }

                        _dbContext.SaveChanges();

                    }
                }
                else
                {
                    Feedback obj = new Feedback { Question = model.Question.Trim(), CreatedBy = currentUserId, LastUpdated = DateTime.Now };
                    _dbContext.Feedback.Add(obj);
                    _dbContext.SaveChanges();
                    var feedbackId = obj.FeedbackId;

                    if (model.Users.Count > 0)
                    {
                        foreach (var user in model.Users)
                        {
                            var ReplyObj = new Replies { FeedbackId = feedbackId, UsersId = user, CreatedBy = currentUserId, LastUpdated = DateTime.Now };
                            _dbContext.Reply.Add(ReplyObj);
                        }
                        _dbContext.SaveChanges();
                    }
                }


                return Ok(new ApiResponse { Error = false, Data = "Feedback created successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { Error = true, Message = ex.Message.ToString() });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var objFromDb = _dbContext.Feedback.Find(id);
                if (objFromDb == null)
                {
                    throw new Exception("No records found !");
                }
                _dbContext.Feedback.Remove(objFromDb);
                _dbContext.SaveChanges();

                return Ok(new ApiResponse { Error = false, Data = "Feedback delete successfully" });

            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { Error = true, Message = ex.Message.ToString() });
            }
        }

        private string GetUserId()
        {
            if (!User.Identity.IsAuthenticated)
                throw new AuthenticationException();

            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

        }


    }
}
