using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SignalApp.Data;
using SignalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalApp
{
    public class SignalRChat: Hub
    {
        private static List<AppUser> connectedUsers = new List<AppUser>();
        private UserManager<IdentityUser> _mgr;
        private IHttpContextAccessor _httpContext;
        private ApplicationDbContext _db;

        public SignalRChat(UserManager<IdentityUser> mgr, IHttpContextAccessor httpContext,
            ApplicationDbContext db)
        {
            _mgr = mgr;
            _httpContext = httpContext;
            _db = db;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
            
        }

        public async Task SendMessageToUser(string conId, string msg)
        {
            string userName = _httpContext.HttpContext.User.Identity.Name;       
            await Clients.Client(conId).SendAsync("ReceiveMessage", userName, msg, Context.ConnectionId);

            string userId = _mgr.GetUserId(_httpContext.HttpContext.User);

            SignalMessage smsg = new SignalMessage();
            smsg.Message = msg;
            smsg.UserId = userId;

            var friend = connectedUsers.Where(x => x.ConnectionId == conId).FirstOrDefault();
            smsg.FriendId = friend.Id;
            smsg.DateCreated = DateTime.Now;

            _db.Add(smsg);
                      

            await _db.SaveChangesAsync();
        }

        public override Task OnConnectedAsync()
        {
            string userId = _mgr.GetUserId(_httpContext.HttpContext.User);
            var checkuser = connectedUsers.Where(x => x.Id == userId).FirstOrDefault();
            if(checkuser != null)
            {
                connectedUsers.Remove(checkuser);
            }

            AppUser user = new AppUser();
            string conId = Context.ConnectionId;
            user.ConnectionId = conId;
            user.Id = _mgr.GetUserId(_httpContext.HttpContext.User);
            connectedUsers.Add(user);

            //Clients.All.SendAsync("UpdateFriendList", userId, conId);
            var json = JsonConvert.SerializeObject(connectedUsers);
            Clients.All.SendAsync("UpdateFriendList2", json);
            return base.OnConnectedAsync();
        }

        public void SendToUser(string userId, string msg)
        {
            var connectionId = connectedUsers.Where(x => x.Id == userId)
                .Select(x => x.ConnectionId).FirstOrDefault();
            
            string myName = _httpContext.HttpContext.User.Identity.Name;
            Clients.Client(connectionId).SendAsync("ReceiveMessage", myName, msg, connectionId);
        }
    }
}
