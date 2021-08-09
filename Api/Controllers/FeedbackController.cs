using Data;
using Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Models.ViewModels;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
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
                var assignee = _dbContext.FeedbackAssigned.Join(_dbContext.Users,
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
                if (model.FeedbackId == 0)
                {
                    Feedback obj = new Feedback { Question = model.Question.Trim(), CreatedBy = currentUserId, LastUpdated = DateTime.Now };
                    _dbContext.Feedback.Add(obj);
                    _dbContext.SaveChanges();
                    var feedbackId = obj.FeedbackId;

                    if (model.Users.Count > 0)
                    {
                        foreach (var user in model.Users)
                        {
                            var feedbackAssignedObj = new FeedbackAssigned { FeedbackId = feedbackId, UsersId = user, CreatedBy = currentUserId, LastUpdated = DateTime.Now };
                            _dbContext.FeedbackAssigned.Add(feedbackAssignedObj);
                        }
                        _dbContext.SaveChanges();
                    }
                }
                else
                {
                    var feedbackObj = _dbContext.Feedback.FirstOrDefault(fb => fb.FeedbackId == model.FeedbackId);
                    if (feedbackObj != null)
                    {

                        feedbackObj.Question = model.Question;
                        _dbContext.SaveChanges();

                        // Updating assinees
                        var existingAssignees = _dbContext.FeedbackAssigned.Where(fba => fba.FeedbackId == model.FeedbackId).Select(fba => fba.UsersId).ToList();

                        //Users need to removed from feedback
                        var removeAssignee = existingAssignees.Except(model.Users).ToList();
                        if (removeAssignee.Count > 0)
                        {
                            foreach (var user in removeAssignee)
                            {
                                var removeAssigneeObj = _dbContext.FeedbackAssigned.Where(fba => fba.UsersId == user && fba.FeedbackId == model.FeedbackId.Value).FirstOrDefault();
                                _dbContext.FeedbackAssigned.Remove(removeAssigneeObj);
                            }
                        }

                        var addAssignee = model.Users.Except(existingAssignees).ToList();

                        if(addAssignee.Count > 0)
                        {
                            foreach (var user in addAssignee)
                            {
                                var feedbackAssignedObj = new FeedbackAssigned { FeedbackId = model.FeedbackId.Value, UsersId = user, CreatedBy = currentUserId, LastUpdated = DateTime.Now };
                                _dbContext.FeedbackAssigned.Add(feedbackAssignedObj);
                            }
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
            //var token = HttpContext.Request.Cookies["refreshToken"];
            //var identityUser = _dbContext.Users.Include(x => x.Tokens)
            //    .FirstOrDefault(x => x.Tokens.Any(y => y.Token == token && y.UserId == x.Id));
            //return identityUser.Id;

            return "9681abb8-ce8a-4eaf-bd3a-d69133018d02";
        }


    }
}
