namespace RailCalcApi.Models
{
    public class TimetableRowDto
    {    
        public float StationPosition { get; set; }
        public string StationName { get; set; }
        public TimeOnly? Arrival { get; set; }
        public TimeOnly? Departure { get; set; }
        public TimeSpan? TravelTime { get; set; }
        public float EnergyConsumption { get; set; }
        public float AverageSpeed { get; set; }
    }
}
