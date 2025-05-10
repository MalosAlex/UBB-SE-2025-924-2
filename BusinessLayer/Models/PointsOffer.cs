using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models
{
    public class PointsOffer
    {
        [Key]
        public int OfferId { get; set; }
        public int Points { get; set; }
        public int Price { get; set; }
        public PointsOffer(int price, int points)
        {
            this.Price = price;
            this.Points = points;
        }
    }
}
