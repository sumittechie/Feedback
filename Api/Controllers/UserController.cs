﻿using Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Models;
using System;
using System.Security.Authentication;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly FeedbackDbContext _dbContext;

        public UserController(FeedbackDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var users = _dbContext.Users
                        .Where(u => u.Id != CurrentUserId())
                        .Select(user => new { id = user.Id, name = $"{user.Name} [{user.Email}]" }).ToList();

                return Ok(new ApiResponse { Error = false, Data = users });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { Error = true, Message = ex.Message.ToString() });
            }
        }

        private string CurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                throw new AuthenticationException();

            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

        }
    }
}
