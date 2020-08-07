using System;

namespace ProtoBuf.Grpc.Configuration
{
    public interface IServiceImplementationMapper
    {
        Type Map(Type serviceType);
    }
}
