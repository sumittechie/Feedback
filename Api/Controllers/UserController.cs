using Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Models;
using System;
using System.Security.Authentication;
using System.Security.Claims;
using Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly FeedbackDbContext _dbContext;
        private readonly UserManager<Users> _userManager;

        public UserController(FeedbackDbContext dbContext, UserManager<Users> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }


        [HttpGet]
        [Route("dropdown")]
        public IActionResult GetDropdownList()
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


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var response = new ApiResponse();
            try
            {
                var users = _dbContext.Users.Select(u => new { Id = u.Id, Name = u.Name, Email = u.Email, Gender = u.Gender, Photo = u.Photo, IsAdmin = u.IsAdmin, Mobile = u.PhoneNumber }).ToList();
                response.Error = false;
                response.Data = users;
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = ex.Message.ToString();
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Get(string id)
        {
            var response = new ApiResponse();
            try
            {
                var user = _dbContext.Users.Where(u => u.Id == id).Select(us => new
                {
                    Id = us.Id,
                    Name = us.Name,
                    Email = us.Email,
                    Mobile = us.PhoneNumber,
                    IsAdmin = us.IsAdmin,
                    Gender = us.Gender
                })
                    .FirstOrDefault();

                response.Error = false;
                response.Data = user;
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = ex.Message.ToString();
            }
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] UserPostVm vm)
        {
            try
            {
                if (!string.IsNullOrEmpty(vm.Id))
                {
                    var user = _dbContext.Users.Where(us => us.Id == vm.Id).FirstOrDefault();
                    if (user != null)
                    {
                        user.IsAdmin = vm.IsAdmin;
                        user.Name = vm.Name;
                        user.UserName = vm.Name.Replace(" ", ".");
                        user.NormalizedUserName = vm.Name.Replace(" ", ".").ToUpper();
                        user.Gender = vm.Gender;
                        user.PhoneNumber = vm.Mobile;
                        user.Email = vm.Email;
                        user.NormalizedEmail = vm.Email.ToUpper();
                        _dbContext.SaveChanges();
                        return Ok(new ApiResponse { Error = false, Data = "User created successfully" });
                    }
                    else
                    {
                        return Ok(new ApiResponse { Error = true, Data = "Invalid user detail for update" });
                    }
                }
                else
                {
                    if (!ModelState.IsValid || vm == null)
                    {
                        return Ok(new ApiResponse { Error = true, Message = "User Registration Failed" });
                    }

                    var genederType = vm.Gender == "Male" ? "men" : "women";
                    var rand = new Random();
                    var photoId = rand.Next(1, 100);

                    var identityUser = new Users()
                    {
                        UserName = vm.Name.Replace(" ", "."),
                        Email = vm.Email,
                        Name = vm.Name,
                        PhoneNumber = vm.Mobile,
                        IsAdmin = false,
                        Gender = vm.Gender,
                        Photo = $"https://randomuser.me/api/portraits/{genederType}/{photoId}.jpg"
                    };
                    var result = await _userManager.CreateAsync(identityUser, vm.Password);
                    if (!result.Succeeded)
                    {
                        List<string> errors = new List<string>();
                        foreach (IdentityError error in result.Errors)
                        {
                            errors.Add(error.Description);
                        }

                        return Ok(new ApiResponse { Error = true, Data = errors });
                    }

                    return Ok(new ApiResponse { Error = false, Data = "User created successfully" });

                }
            }
            catch (Exception ex)
            {
                var response = new ApiResponse();
                response.Error = true;
                response.Message = ex.Message.ToString();

                return Ok(response);
            }

        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var response = new ApiResponse();

            try
            {
                var user = _dbContext.Users.Where(u => u.Id == id).FirstOrDefault();
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    _dbContext.SaveChanges();
                    response.Error = false;
                    response.Data = "User deleted successfully";
                }
                else
                {
                    response.Error = true;
                    response.Data = "User not found for delete";
                }
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = ex.Message.ToString();
            }
            return Ok(response);

        }
        private string CurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                throw new AuthenticationException();

            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

        }
    }
}
