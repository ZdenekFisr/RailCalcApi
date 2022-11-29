namespace RailCalcApi.Models
{
    public class TimetableRow
    {
        private readonly double _stationPosition;
        private readonly string _stationName;
        private readonly string? _arrival;
        private readonly string? _departure;
        private readonly string? _travelTime;
        private readonly double? _energyConsumption;
        private readonly double? _averageSpeed;
        
        public double StationPosition { get => _stationPosition; }
        public string StationName { get => _stationName; }
        public string? Arrival { get => _arrival; }
        public string? Departure { get => _departure; }
        public string? TravelTime { get => _travelTime; }
        public double? EnergyConsumption { get => _energyConsumption; }
        public double? AverageSpeed { get => _averageSpeed; }

        public TimetableRow(TimetableRowDto dto)
        {
            _stationPosition = dto.StationPosition;
            _stationName = dto.StationName;
            _arrival = dto.Arrival?.ToString("h:mm:ss");
            _departure = dto.Departure?.ToString("h:mm:ss");
            _travelTime = dto.TravelTime != null ? new TimeSpan((long)dto.TravelTime * 10000000).ToString("c") : null;
            _energyConsumption = dto.EnergyConsumption;
            _averageSpeed = dto.AverageSpeed;
        }
    }
}
