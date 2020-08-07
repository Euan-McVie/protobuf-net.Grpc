using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
namespace Shared_CS
{
    [ServiceContract(Name = "Hyper.Calculator")]
    public interface ICalculator
    {
        ValueTask<MultiplyResult> MultiplyAsync(MultiplyRequest request);
        ValueTask<IncrementResult> IncrementAsync(IncrementRequest request);
    }

    [DataContract]
    public class MultiplyRequest
    {
        [DataMember(Order = 1)]
        public int X { get; set; }

        [DataMember(Order = 2)]
        public int Y { get; set; }
    }

    [DataContract]
    public class MultiplyResult
    {
        [DataMember(Order = 1)]
        public int Result { get; set; }
    }

    [DataContract]
    public class IncrementRequest
    {
        [DataMember(Order = 1)]
        public int Inc { get; set; }
    }

    [DataContract]
    public class IncrementResult
    {
        [DataMember(Order = 1)]
        public int Result { get; set; }
    }
}
