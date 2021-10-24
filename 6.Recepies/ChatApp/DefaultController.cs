using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp
{
    [Authorize]
    public class DefaultController : ControllerBase
    {
        private readonly ChatRegistry _chatRegistry;

        public DefaultController(ChatRegistry chatRegistry) => _chatRegistry = chatRegistry;

        [AllowAnonymous]
        [HttpGet("/auth")]
        public IActionResult Authenticate(string username)
        {
            var claims = new Claim[]
            {
                new("user_id", Guid.NewGuid().ToString()),
                new("username", username),
            };

            var identity = new ClaimsIdentity(claims, "Cookie");
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync("Cookie", principal);
            return Ok();
        }

        [HttpGet("/create")]
        public IActionResult CreateRoom(string room)
        {
            _chatRegistry.CreateRoom(room);
            return Ok();
        }

        [HttpGet("/list")]
        public IActionResult ListRooms()
        {
            return Ok(_chatRegistry.GetRooms());
        }
    }
}