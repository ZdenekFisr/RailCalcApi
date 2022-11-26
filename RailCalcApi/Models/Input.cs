namespace RailCalcApi.Models
{
    public class Input
    {
        public RailRoute RailRoute { get; set; }
        public Train Train { get; set; }
        /// <summary>
        /// Ratio of vertical wheel force and maximum possible tangent force
        /// 
        /// Recommended values: 0.3 to 0.35 (lower when rails are slippery)
        /// </summary>
        public float AdhesionLimit { get; set; }
        /// <summary>
        /// Deceleration in m/s^2 when the train is braking
        /// 
        /// Recommended values: 0.45 to 0.7
        /// </summary>
        public float BrakesDeceleration { get; set; }
        /// <summary>
        /// Format h:mm:ss
        /// </summary>
        public string InitialTime { get; set; }
        public string TimeOfStandingInStations { get; set; }
        public bool ReversedDirectionOfTravel { get; set; } = false;
    }
}
