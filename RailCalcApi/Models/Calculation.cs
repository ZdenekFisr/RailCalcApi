using static System.Collections.Specialized.BitVector32;

namespace RailCalcApi.Models
{
    public class Calculation
    {
        private const double segmentLength = 0.01;
        private const double gravAcceleration = 9.81;

        public static Output Execute(InputDto input)
        {
            List<TimetableRowDto> timetableDto = new();

            Segment[] segments = new Segment[(int)(input.RailRoute.Length / segmentLength) + 1];

            List<LineSection> longitudalProfile = new();

            #region Preprocessing

            // create segment array (i. e. numerical table)
            double km, previousPosition;
            int directionFactor = input.ReversedDirectionOfTravel ? -1 : 1;

            previousPosition = input.ReversedDirectionOfTravel ? input.RailRoute.Stations.Last().Position : input.RailRoute.Stations.First().Position;
            for (int i = 0; i < segments.Length; i++)
            {
                km = Math.Round(i * segmentLength, 3);
                segments[i] = new Segment { Position = previousPosition + directionFactor * km, Km = km };
            }

            // calculate altitude profile
            double x, y = 0, positionX, positionY = 0;
            int count = 0;
            foreach (Station station in input.RailRoute.Stations)
            {
                if (station.Altitude == null) continue;
                if (count != 2) count++;
                x = y;
                positionX = positionY;
                y = (double)station.Altitude;
                positionY = station.Position;
                if (count == 2) longitudalProfile.Add(new LineSection { From = positionX, To = positionY, Value = Math.Round((y - x) / (positionY - positionX), 2) });
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
                segments[i].InclineResistance = input.Train.Weight * gravAcceleration * segments[i].Incline / 1000;
            }

            #endregion

            #region Deceleration profile

            double speedMs = 0, speedKmh;

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

            #endregion

            #region Acceleration profile, final profile

            speedMs = 0;
            speedKmh = 0;
            double acceleration, actualAcceleration, totalTime = 0, totalEnergy = 0, previousTime = 0, previousEnergy = 0,
                wheelForce, adhesionForce = input.Train.AdhesionWeight * gravAcceleration * input.AdhesionLimit;
            Station? currentStation;

            timetableDto.Add(new() { StationName = input.RailRoute.Stations.First().ToString(), Departure = input.InitialTime });

            for (int i = 0; i < segments.Length; i++)
            {
                currentStation = input.RailRoute.Stations.Find(s => s.Position == segments[i].Position);

                if (currentStation is not null) speedMs = 0;
                speedKmh = speedMs * 3.6;
                segments[i].SpeedUp = speedKmh;
                segments[i].Speed = Math.Min(segments[i].SpeedDown, segments[i].SpeedUp);

                segments[i].TrainResistance = (input.Train.ResistanceConstant + input.Train.ResistanceLinear * segments[i].Speed + input.Train.ResistanceQuadratic * segments[i].Speed * segments[i].Speed) * input.Train.Weight * gravAcceleration / 1000;

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
                if ((segments[i].SpeedUp == 0 && segments[i].SpeedDown == 0) || segments[i].SpeedUp < segments[i].SpeedDown) segments[i].Acceleration = acceleration; // train is standing in a station or accelerating
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
                    totalEnergy += segments[i].Work;
                }

                // add timetable row
                if (i != 0 && currentStation is not null)
                {
                    TimeOnly? departure = i != segments.Length - 1 ? input.InitialTime.AddMinutes((totalTime + input.TimeOfStandingInStations.TotalSeconds) / 60) : null;
                    timetableDto.Add(new()
                    {
                        StationPosition = segments[i].Position,
                        StationName = currentStation.ToString(),
                        Arrival = input.InitialTime.AddMinutes(totalTime / 60),
                        Departure = departure,
                        TravelTime = totalTime - previousTime,
                        EnergyConsumption = Math.Round(totalEnergy / (input.Train.Efficiency * 3600000) - previousEnergy, 2),
                        AverageSpeed = Math.Round(Math.Abs(segments[i].Position - previousPosition) * 3600 / (totalTime - previousTime), 2)
                    });
                    totalTime += input.TimeOfStandingInStations.TotalSeconds;
                    previousTime = totalTime;
                    previousEnergy = totalEnergy / (input.Train.Efficiency * 3600000);
                }
            }

            totalEnergy /= input.Train.Efficiency * 3600000;

            #endregion

            return new Output(timetableDto, totalTime, totalEnergy, input.RailRoute.Length, input.TimeOfStandingInStations, segments.Max(s => s.Speed));
        }
    }
}
