using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Shared_CS;

namespace Services_CS
{
    [Authorize]
    public class MyCalculator : ICalculator
    {
        private int counter = 0;

        ValueTask<MultiplyResult> ICalculator.MultiplyAsync(MultiplyRequest request)
        {
            var result = new MultiplyResult { Result = request.X * request.Y };
            return new ValueTask<MultiplyResult>(result);
        }

        ValueTask<IncrementResult> ICalculator.IncrementAsync(IncrementRequest request)
        {
            counter += request.Inc;
            var result = new IncrementResult { Result = counter };
            return new ValueTask<IncrementResult>(result);
        }
    }
}
