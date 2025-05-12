using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class ChatUser
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;

        public ICollection<ChatInvite> SentInvites { get; set; } = new List<ChatInvite>();
        public ICollection<ChatInvite> ReceivedInvites { get; set; } = new List<ChatInvite>();
    }
}
