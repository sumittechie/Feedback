using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class FeedbackController : ControllerBase
    {

        private readonly FeedbackDbContext _dbContext;

        public FeedbackController(FeedbackDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var feedbacks = _dbContext.Feedback.ToList();
                return Ok(new { HasEror = false, Message = feedbacks });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { HasError = true, Message = ex.Message.ToString() });
            }
        }

        [HttpGet("{id}")]        
        public IActionResult Get(int id)
        {
            try
            {
                var feedback = _dbContext.Feedback.FirstOrDefault(row => row.FeedbackId == id);
                return Ok(new { HasEror = false, Message = feedback });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { HasError = true, Message = ex.Message.ToString() });
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
                return Ok(new { HasEror = false, Message = "Feedback created successfully" });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { HasError = true, Message = ex.Message.ToString() });
            }

        }

        [HttpDelete]
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

                return Ok(new { HasError = false, Message = "Successfully deleted" });

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { HasError = true, Message = ex.Message.ToString() });
            }
        }

        private string GetUserId()
        {
            var token = HttpContext.Request.Cookies["refreshToken"];
            var identityUser = _dbContext.Users.Include(x => x.Tokens)
                .FirstOrDefault(x => x.Tokens.Any(y => y.Token == token && y.UserId == x.Id));

            return identityUser.Id;
        }


    }
}
