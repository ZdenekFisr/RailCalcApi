using System.ComponentModel.DataAnnotations;

namespace RailCalcApi.Models
{
    public class Station
    {
        public float Position { get; set; }
        [Required]
        public string Name { get; set; }
        public float? Altitude { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
