using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignalApp.Data;
using SignalApp.Models;

namespace SignalApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private UserManager<IdentityUser> _mgr;
        private ApplicationDbContext _db;
        public HomeController(UserManager<IdentityUser> mgr, ApplicationDbContext db)
        {
            _mgr = mgr;
            
            _db = db;
        }

        public IActionResult Search(string query)
        {
            
            var result = _db.Users.Where(x => x.UserName.StartsWith(query)).ToList();
            return View(result);
        }

        [HttpPost]
        public JsonResult AddFriend(string id)
        {
            FriendList list = new FriendList();
            list.FriendId = id;
            //zalogowany user ID
            string userId = _mgr.GetUserId(HttpContext.User);

            list.UserId = userId;
            if(!_db.FriendList.Any(x=>x.UserId == userId && x.FriendId == id) 
                && !_db.FriendList.Any(x => x.UserId == id && x.FriendId == userId))
            {
                _db.FriendList.Add(list);
                _db.SaveChanges();
            }         

            return Json(true);
        }

        public JsonResult GetMsgs(string friendId)
        {
            string userId = _mgr.GetUserId(HttpContext.User);
            var list = _db.SignalMessages
                .Where(x => x.UserId == userId || x.FriendId == userId)
                .OrderBy(x => x.DateCreated).ToList();
            return Json(list);

        }

        public IActionResult Index()
        {
            string userId = _mgr.GetUserId(HttpContext.User);
            //Lista znajomych których zaprosiłem
            var friendList = _db.FriendList.Where(x => x.UserId == userId).Select(x => x.FriendId).ToList();
            //Lista znajomych którzy mnie zaprosili
            var requestList = _db.FriendList.Where(x => x.FriendId == userId).Select(x => x.UserId).ToList();

            var friends = _mgr.Users.Where(x => friendList.Contains(x.Id) || requestList.Contains(x.Id)).ToList();

            return View(friends);
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
