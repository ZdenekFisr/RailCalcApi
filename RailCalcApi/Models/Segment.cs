namespace RailCalcApi.Models
{
    public class Segment
    {
        public float Position { get; set; }
        public double Km { get; set; }
        public double Incline { get; set; }
        public double InclineResistance { get; set; }
        public double TrainResistance { get; set; }
        public double SpeedUp { get; set; }
        public double SpeedDown { get; set; }
        public double AccelerationDown { get; set; }
        public double Speed { get; set; }
        public double Acceleration { get; set; }
        public int AllowedSpeed { get; set; }
        public double PullForce { get; set; }
        public double Time { get; set; }
        public double Work { get; set; }
    }
}
