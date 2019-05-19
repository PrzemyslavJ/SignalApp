using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalApp.Models
{
    public class FriendList
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FriendId { get; set; }
    }
}
