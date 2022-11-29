using System.ComponentModel.DataAnnotations;

namespace RailCalcApi.Models
{
    public class Train
    {
        public double Weight { get; set; }
        public double MaxSpeed { get; set; }
        /// <summary>
        /// Part of the weight that is carried by driving wheels
        /// </summary>
        public double AdhesionWeight { get; set; }
        /// <summary>
        /// Addition to kinetic energy made by rotating parts (wheels, axle, drive) expressed as weight (~2% of M for pulled cars, ~20% of M for driving cars)
        /// </summary>
        public double MassEquivalentToRotatingParts { get; set; }
        /// <summary>
        /// Constant part of resistance against motion - independent of speed (usually between 1 and 3)
        /// </summary>
        [Range(1, 3)]
        public double ResistanceConstant { get; set; }
        /// <summary>
        /// Linear part of resistance against motion (usually 0 or up to 0.002)
        /// </summary>
        [Range(0, 0.002)]
        public double ResistanceLinear { get; set; }
        /// <summary>
        /// Quadratic part of resistance against motion - mostly air resistance (usually between 0.0002 and 0.001)
        /// </summary>
        [Range(0.0002, 0.001)]
        public double ResistanceQuadratic { get; set; }
        public double Performance { get; set; }
        public double MaxPullForce { get; set; }
        [Range(0, 1)]
        public double Efficiency { get; set; }
    }
}
