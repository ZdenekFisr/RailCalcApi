using static System.Collections.Specialized.BitVector32;

namespace RailCalcApi.Models
{
    public class Calculation
    {
        private const float segmentLength = 0.01F;
        private const double gravAcceleration = 9.81;

        public static Output Execute(Input input)
        {
            List<TimetableRowDto> timetableDto = new();

            input.RailRoute.Stations.Sort((s1, s2) => s1.Position.CompareTo(s2.Position));

            Segment[] segments = new Segment[(int)(input.RailRoute.Length / segmentLength) + 1];

            List<LineSection> longitudalProfile = new();

            if (!TimeOnly.TryParse(input.InitialTime, out TimeOnly initialTime)) throw new InvalidDataException("Initial time is not in correct format. The correct format is 'h:mm:ss'.");
            if (!TimeSpan.TryParse(input.TimeOfStandingInStations, out TimeSpan timeOfStandingInStations)) throw new InvalidDataException("Time of standing in stations is not in correct format. The correct format is 'h:mm:ss'.");
            #region Preprocessing

            // create segment array (i. e. numerical table)
            float km, previousPosition;
            int directionFactor = input.ReversedDirectionOfTravel ? -1 : 1;

            previousPosition = input.ReversedDirectionOfTravel ? input.RailRoute.Stations.Last().Position : input.RailRoute.Stations.First().Position;
            for (int i = 0; i < segments.Length; i++)
            {
                km = i * segmentLength;
                segments[i] = new Segment { Position = previousPosition + directionFactor * km, Km = km };
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

            double speedMs = 0, speedKmh;

            // deceleration profile (down)
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                if (input.RailRoute.Stations.Exists(s => s.Position == segments[i].Position)) speedMs = 0;
                speedKmh = speedMs * 3.6;
                segments[i].SpeedDown = speedKmh;

                if (speedKmh >= segments[i].AllowedSpeed) // constant speed
                {
                    segments[i].AccelerationDown = 0;
                    speedMs = segments[i].AllowedSpeed / 3.6;
                }

                else // deceleration
                {
                    segments[i].AccelerationDown = -input.BrakesDeceleration;
                    speedMs = Math.Sqrt(speedMs * speedMs + 2 * input.BrakesDeceleration * segmentLength * 1000);
                }
            }

            // acceleration profile (up), final profile
            speedMs = 0;
            speedKmh = 0;
            double acceleration, actualAcceleration, totalTime = 0, totalWork = 0, previousTime = 0, previousWork = 0,
                wheelForce, adhesionForce = input.Train.AdhesionWeight * gravAcceleration * input.AdhesionLimit;
            Station? currentStation;

            timetableDto.Add(new() { StationName = input.RailRoute.Stations.First().ToString(), Departure = initialTime });

            for (int i = 0; i < segments.Length; i++)
            {
                currentStation = input.RailRoute.Stations.Find(s => s.Position == segments[i].Position);

                if (currentStation is not null) speedMs = 0;
                speedKmh = speedMs * 3.6;
                segments[i].SpeedUp = speedKmh;
                segments[i].Speed = Math.Min(segments[i].SpeedDown, segments[i].SpeedUp);

                segments[i].TrainResistance = (input.Train.ResistanceConstant + input.Train.ResistanceLinear * segments[i].Speed + input.Train.ResistanceQuadratic * segments[i].Speed * segments[i].Speed) / 1000;

                wheelForce = Math.Min(adhesionForce, Math.Min(input.Train.MaxPullForce, input.Train.Performance / speedMs));
                
                if (speedKmh < segments[i].AllowedSpeed) // train uses full force (it may accelerate or decelerate)
                {
                    acceleration = (wheelForce - segments[i].TrainResistance - segments[i].InclineResistance) / (input.Train.Weight + input.Train.MassEquivalentToRotatingParts);
                    speedMs = Math.Sqrt(speedMs * speedMs + 2 * acceleration * segmentLength * 1000);
                }
                else // constant speed
                {
                    acceleration = 0;
                    speedMs = segments[i].AllowedSpeed / 3.6;
                }

                // final acceleration
                if ((segments[i].SpeedUp == 0 && segments[i].SpeedDown == 0) || segments[i].SpeedUp > segments[i].SpeedDown) segments[i].Acceleration = acceleration; // train is standing in a station or accelerating
                else if (segments[i].SpeedUp == segments[i].SpeedDown) segments[i].Acceleration = 0; // constant speed equal to allowed speed
                else segments[i].Acceleration = segments[i].AccelerationDown; // train is decelerating

                // pull force
                segments[i].PullForce = (i == segments.Length - 1) ? 0 : (input.Train.Weight + input.Train.MassEquivalentToRotatingParts) * segments[i].Acceleration + segments[i].TrainResistance + segments[i].InclineResistance;

                // time and work
                if (i != 0) // first segment has no length
                {
                    actualAcceleration = (Math.Pow(segments[i].Speed / 3.6, 2) - Math.Pow(segments[i - 1].Speed / 3.6, 2)) / (2000 * segmentLength);
                    segments[i].Time = (actualAcceleration == 0) ? segmentLength * 1000 / (segments[i].Speed / 3.6) : (segments[i].Speed / 3.6 - segments[i - 1].Speed / 3.6) / actualAcceleration;
                    totalTime += segments[i].Time;
                    if (segments[i - 1].PullForce > 0) segments[i].Work = segments[i - 1].PullForce * segmentLength * 1000000;
                    totalWork += segments[i].Work;
                }

                // add timetable row
                if (i != 0 && currentStation is not null)
                {
                    TimeOnly? departure = i != segments.Length ? initialTime.AddMinutes((totalTime + timeOfStandingInStations.TotalSeconds) / 60) : null;
                    timetableDto.Add(new()
                    {
                        StationPosition = segments[i].Position,
                        StationName = currentStation.ToString(),
                        Arrival = initialTime.AddMinutes(totalTime / 60),
                        Departure = departure,
                        TravelTime = totalTime - previousTime,
                        EnergyConsumption = totalWork - previousWork,
                        AverageSpeed = Math.Abs(segments[i].Position - previousPosition) / (float)(totalTime - previousTime)
                    });
                    totalTime += timeOfStandingInStations.TotalSeconds;
                    previousTime = totalTime;
                    previousWork = totalWork;
                }
            }

            #endregion

            #region Postprocessing

            #endregion

            return new Output(timetableDto, timeOfStandingInStations, segments.Max(s => (float)s.Speed));
        }
    }
}
