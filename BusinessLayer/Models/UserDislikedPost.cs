using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class UserDislikedPost
    {
        public int UserId { get; set; }
        public uint PostId { get; set; }
    }
}
