using System.ComponentModel.DataAnnotations;

namespace RailCalcApi.Models
{
    public class Station
    {
        public double Position { get; set; }
        [Required]
        public string Name { get; set; }
        public double? Altitude { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
