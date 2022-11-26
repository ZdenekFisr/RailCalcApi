namespace RailCalcApi.Models
{
    public class Calculation
    {
        private const double segmentLength = 0.01;
        private const double gravAcceleration = 9.81;

        public static Output Execute(Input input)
        {
            List<TimetableRowDto> timetableDto = new();

            input.RailRoute.Stations.Sort((s1, s2) => s1.Position.CompareTo(s2.Position));

            Segment[] segments = new Segment[(int)(input.RailRoute.Length / segmentLength) + 1];

            List<LineSection> longitudalProfile = new();

            float maxSpeed = 0;

            #region Preprocessing

            // create segment array (i. e. numerical table)
            double km;
            if (!input.ReversedDirectionOfTravel)
            {
                for (int i = 0; i < segments.Length; i++)
                {
                    km = i * segmentLength;
                    segments[i] = new Segment { Position = input.RailRoute.Stations.First().Position + km, Km = km };
                }
            }
            else
            {
                for (int i = 0; i < segments.Length; i++)
                {
                    km = i * segmentLength;
                    segments[i] = new Segment { Position = input.RailRoute.Stations.Last().Position - km, Km = km };
                }
            }

            // calculate altitude profile
            float x, y = 0, positionX, positionY = 0;
            int count = 0;
            foreach (Station station in input.RailRoute.Stations)
            {
                if (station.Altitude == null) continue;
                if (count != 2) count++;
                x = y;
                positionX = positionY;
                y = (float)station.Altitude;
                positionY = station.Position;
                if (count == 2) longitudalProfile.Add(new LineSection { From = positionX, To = positionY, Value = (float)Math.Round((y - x) / (positionY - positionX), 2) });
            }

            // assign constants
            double? speedLimit, incline;
            int directionFactor = input.ReversedDirectionOfTravel ? -1 : 1;
            for (int i = 0; i < segments.Length; i++)
            {
                // allowed speed
                speedLimit = input.RailRoute.SpeedSections.Find(x => x.From <= segments[i].Position && x.To >= segments[i].Position)?.Value;
                if (speedLimit == null) segments[i].AllowedSpeed = input.RailRoute.Speed;
                else segments[i].AllowedSpeed = speedLimit < input.RailRoute.Speed ? (int)speedLimit : input.RailRoute.Speed;

                // incline
                incline = longitudalProfile.Find(x => x.From <= segments[i].Position && x.To >= segments[i].Position)?.Value * directionFactor;
                if (incline != null) segments[i].Incline = incline.Value;

                // incline resistance
                segments[i].InclineResistance = input.Train.Weight * gravAcceleration * segments[i].Incline;
            }

            #endregion

            #region Processing

            #endregion

            #region Postprocessing

            #endregion

            return new Output(timetableDto, maxSpeed);
        }
    }
}
