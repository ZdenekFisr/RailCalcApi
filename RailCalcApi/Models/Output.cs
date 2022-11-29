using Microsoft.AspNetCore.Mvc;

namespace RailCalcApi.Models
{
    public class Output
    {
        private readonly List<TimetableRow> _timetable;
        private readonly TimeSpan _totalTime;
        private readonly double _totalEnergyConsumption;
        private readonly double _maxSpeed;
        private readonly double _averageSpeed;
        private readonly double _averageTravelSpeed;

        public List<TimetableRow> Timetable { get => _timetable; }
        public string Time { get => _totalTime.ToString("c"); }
        public double EnergyConsumption { get => _totalEnergyConsumption; }
        public double MaxSpeed { get => _maxSpeed; }
        public double AverageSpeed { get => _averageSpeed; }
        public double AverageTravelSpeed { get => _averageTravelSpeed; }

        public Output(IEnumerable<TimetableRowDto> timetableDto, double totalTimeInSeconds, double totalEnergy, double railRouteLength, TimeSpan timeOfStandingInStations, double maxSpeed)
        {
            _timetable = new();
            _totalTime = new TimeSpan(0);

            foreach (TimetableRowDto timetableRowDto in timetableDto)
            {
                _timetable.Add(new TimetableRow(timetableRowDto));
            }
            _totalTime = new TimeSpan((long)totalTimeInSeconds * 10000000);
            _totalEnergyConsumption = Math.Round(totalEnergy, 2);
            _maxSpeed = Math.Round(maxSpeed, 2);
            _averageSpeed = Math.Round(railRouteLength * 3600 / (totalTimeInSeconds - (timetableDto.Count() - 1) * timeOfStandingInStations.TotalSeconds), 2);
            _averageTravelSpeed = Math.Round(railRouteLength * 3600 / totalTimeInSeconds, 2);
        }
    }
}
