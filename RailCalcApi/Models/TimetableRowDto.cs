namespace RailCalcApi.Models
{
    public class TimetableRowDto
    {    
        public float StationPosition { get; set; }
        public string StationName { get; set; }
        public TimeOnly? Arrival { get; set; }
        public TimeOnly? Departure { get; set; }
        public double TravelTime { get; set; }
        public double EnergyConsumption { get; set; }
        public float AverageSpeed { get; set; }
    }
}
