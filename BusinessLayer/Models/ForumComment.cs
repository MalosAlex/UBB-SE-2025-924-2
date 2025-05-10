using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class ForumComment
    {
        public uint Id { get; set; }
        public string Body { get; set; }
        public int Score { get; set; }
        public DateTime TimeStamp { get; set; }
        public uint AuthorId { get; set; }

        public uint PostId { get; set; }
    }
}
