using Microsoft.AspNetCore.Mvc;
using RailCalcApi.Models;

namespace RailCalcApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : Controller
    {
        [HttpPost("calculate")]
        public Output Calculate(Input input)
        {
            return Calculation.Execute(input);
        }
    }
}
