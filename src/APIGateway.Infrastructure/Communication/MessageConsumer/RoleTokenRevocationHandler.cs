using APIGateway.Application;
using Common.Application.Contracts.Communication.Messages;
using TGF.CA.Infrastructure.Comm.Consumer.Handler;
using TGF.CA.Infrastructure.Comm.Messages;

namespace APIGateway.Infrastructure.Communication.MessageConsumer
{
    public class RoleTokenRevocationHandler(ITokenRevocationService aTokenRevocationService) 
        : IIntegrationMessageHandler<RoleTokenRevoked>
    {
        public async Task Handle(IntegrationMessage<RoleTokenRevoked> aIntegrationMessage, CancellationToken aCancellationToken = default)
            => await aTokenRevocationService.OutdateByDiscordRoleListAsync(aIntegrationMessage.Content.RoleIdList, aCancellationToken);
    }
}
