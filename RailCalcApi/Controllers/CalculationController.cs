using Microsoft.AspNetCore.Mvc;
using RailCalcApi.Models;

namespace RailCalcApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : Controller
    {
        [HttpPost("calculate")]
        public ActionResult<Output> Calculate(Input input)
        {
            #region Validate input
            
            if (input == null) return Problem("Failed to load the input - it is null.");

            if (input.RailRoute.Stations.Count < 2) return Problem("The rail route must contain at least 2 stations.");

            if (input.RailRoute.Speed <= 0) return ProblemPositiveValue("Speed of the rail route");

            foreach (LineSection speedSection in input.RailRoute.SpeedSections)
            {
                if (speedSection.Value <= 0) return ProblemPositiveValue("Each speed section");
            }

            if (input.Train.Weight <= 0) return ProblemPositiveValue("Train weight");
            if (input.Train.MaxSpeed <= 0) return ProblemPositiveValue("Train maximum speed");
            if (input.Train.AdhesionWeight <= 0) return ProblemPositiveValue("Train adhesion weight");
            if (input.Train.MassEquivalentToRotatingParts < 0) return ProblemNotNegativeValue("Train mass equivalent to rotating parts");
            if (input.Train.Performance <= 0) return ProblemPositiveValue("Train performance");
            if (input.Train.MaxPullForce <= 0) return ProblemPositiveValue("Train maximum pull force");
            if (input.Train.Efficiency == 0) return Problem("Train efficiency cannot be zero.");

            if (input.AdhesionLimit <= 0) return ProblemPositiveValue("Adhesion limit");
            if (input.BrakesDeceleration <= 0) return ProblemPositiveValue("Brakes deceleration");

            #endregion

            #region Call calculation and return output

            return Calculation.Execute(new(input));

            #endregion
        }

        private ObjectResult ProblemPositiveValue(string property)
        {
            return Problem(property + " must have a positive value.");
        }

        private ObjectResult ProblemNotNegativeValue(string property)
        {
            return Problem(property + " cannot be a negative value.");
        }
    }
}
