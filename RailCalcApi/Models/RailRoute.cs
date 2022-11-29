namespace RailCalcApi.Models
{
    public class RailRoute
    {
        /// <summary>
        /// Key: position on the line
        /// Value: name of the station
        /// </summary>
        public List<Station> Stations { get; set; } = new();
        public int Speed { get; set; }
        public List<LineSection> SpeedSections { get; set; } = new();

        // Readonly properties
        public double Length { get => Math.Abs(Stations.Last().Position - Stations.First().Position); }

    }
}
