namespace RailCalcApi.Models
{
    public class TimetableRowDto
    {    
        public double StationPosition { get; set; }
        public string StationName { get; set; }
        public TimeOnly? Arrival { get; set; }
        public TimeOnly? Departure { get; set; }
        public double? TravelTime { get; set; }
        public double? EnergyConsumption { get; set; }
        public double? AverageSpeed { get; set; }
    }
}
