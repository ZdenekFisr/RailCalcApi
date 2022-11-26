namespace RailCalcApi.Models
{
    public class TimetableRow
    {
        private readonly float _stationPosition;
        private readonly string _stationName;
        private readonly string? _arrival;
        private readonly string? _departure;
        private readonly string? _travelTime;
        private readonly float _energyConsumption;
        private readonly float _averageSpeed;
        
        public float StationPosition { get => _stationPosition; }
        public string StationName { get => _stationName; }
        public string? Arrival { get => _arrival; }
        public string? Departure { get => _departure; }
        public string? TravelTime { get => _travelTime; }
        public float EnergyConsumption { get => _energyConsumption; }
        public float AverageSpeed { get => _averageSpeed; }

        public TimetableRow(TimetableRowDto dto)
        {
            _stationPosition = dto.StationPosition;
            _stationName = dto.StationName;
            _arrival = dto.Arrival.ToString();
            _departure = dto.Departure.ToString();
            _travelTime = dto.TravelTime.ToString();
            _energyConsumption = dto.EnergyConsumption;
            _averageSpeed = dto.AverageSpeed;
        }
    }
}
