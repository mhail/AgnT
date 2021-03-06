using System;
using MediatR;
using AgentR.Server;
using Server.Requests;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Handlers {

  // These shim handlers are needed to work out of the box using the built 
  // in Dependency Container, they relay the request to a client via the AgentHandler. 
  public class SampleRequestHandler : AgentHandler<SampleRequest, Unit> {
      public SampleRequestHandler(IHubContext<AgentHub> hub) : base(hub) {}
  }

  public class SampleRequestHandler2 : AgentHandler<SampleRequest2, Unit> {
      public SampleRequestHandler2(IHubContext<AgentHub> hub) : base(hub) {}
  }


    public class ServerInfoRequestHandler : IRequestHandler<ServerInfoRequest, ServerInfo>
    {
        public Task<ServerInfo> Handle(ServerInfoRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ServerInfo { 
                Key = "OpenSesame",
            });
        }
    }
}