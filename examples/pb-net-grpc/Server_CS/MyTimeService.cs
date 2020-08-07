using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MegaCorp;
using ProtoBuf.Grpc;
using Shared_CS;

namespace Server_CS
{
    public class MyTimeService : ITimeService
    {
        private readonly ICalculator calculator;

        public MyTimeService(ICalculator calculator)
        {
            this.calculator = calculator;
        }

        public IAsyncEnumerable<TimeResult> SubscribeAsync(CallContext context = default)
            => SubscribeAsyncImpl(context.CancellationToken);

        private async IAsyncEnumerable<TimeResult> SubscribeAsyncImpl([EnumeratorCancellation] CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    var delaytime = await calculator.IncrementAsync(new IncrementRequest { Inc = 2 });
                    Console.WriteLine($"Current time increment: {delaytime.Result}");
                    await Task.Delay(TimeSpan.FromSeconds(delaytime.Result), cancel);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                yield return new TimeResult { Time = DateTime.UtcNow };
            }
        }
    }
}
