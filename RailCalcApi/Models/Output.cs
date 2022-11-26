namespace RailCalcApi.Models
{
    public class Output
    {
        private readonly List<TimetableRow> _timetable;
        private readonly TimeSpan _totalTime;
        private readonly float _totalEnergyConsumption;
        private readonly float _maxSpeed;
        private readonly float _averageSpeed;
        private readonly float _averageTravelSpeed;

        public List<TimetableRow> Timetable { get => _timetable; }
        public string TotalTime { get => _totalTime.ToString(); }
        public float EnergyConsumption { get => _totalEnergyConsumption; }
        public float MaxSpeed { get => _maxSpeed; }
        public float AverageSpeed { get => _averageSpeed; }
        public float AverageTravelSpeed { get => _averageTravelSpeed; }

        public Output(IEnumerable<TimetableRowDto> timetableDto, float maxSpeed)
        {
            _timetable = new();
            _totalTime = new TimeSpan(0);

            foreach (TimetableRowDto timetableRowDto in timetableDto)
            {
                _timetable.Add(new TimetableRow(timetableRowDto));
                if (timetableRowDto.TravelTime != null) _totalTime.Add((TimeSpan)timetableRowDto.TravelTime);
            }
            _totalEnergyConsumption = timetableDto.Sum(x => x.EnergyConsumption);
            _maxSpeed = maxSpeed;
        }
    }
}
