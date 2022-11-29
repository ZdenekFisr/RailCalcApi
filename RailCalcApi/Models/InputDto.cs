namespace RailCalcApi.Models
{
    public class InputDto
    {
        private RailRoute _railRoute;
        private Train _train;
        private double _adhesionLimit;
        private double _brakesDeceleration;
        private TimeOnly _initialTime;
        private TimeSpan _timeOfStandingInStations;
        private bool _reversedDirectionOfTravel;

        public RailRoute RailRoute { get => _railRoute; }
        public Train Train { get => _train; }
        public double AdhesionLimit { get => _adhesionLimit; }
        public double BrakesDeceleration { get => _brakesDeceleration; }
        public TimeOnly InitialTime { get => _initialTime; }
        public TimeSpan TimeOfStandingInStations { get => _timeOfStandingInStations; }
        public bool ReversedDirectionOfTravel { get => _reversedDirectionOfTravel; }

        public InputDto(Input input)
        {
            _railRoute = input.RailRoute;
            _train = input.Train;
            _adhesionLimit = input.AdhesionLimit;
            _brakesDeceleration= input.BrakesDeceleration;
            _initialTime = TimeOnly.Parse(input.InitialTime);
            _timeOfStandingInStations = TimeSpan.Parse(input.TimeOfStandingInStations);
            _reversedDirectionOfTravel = input.ReversedDirectionOfTravel;
        }
    }
}
