using APIGateway.Application;
using Common.Infrastructure.Communication.Messages;
using TGF.CA.Infrastructure.Communication.Consumer.Handler;
using TGF.CA.Infrastructure.Communication.Messages;

namespace APIGateway.Infrastructure.Communication.MessageConsumer
{
    public class RoleTokenRevocationHandler(ITokenRevocationService aTokenRevocationService) 
        : IIntegrationMessageHandler<RoleTokenRevoked>
    {
        public async Task Handle(IntegrationMessage<RoleTokenRevoked> aIntegrationMessage, CancellationToken aCancellationToken = default)
            => await aTokenRevocationService.OutdateByDiscordRoleListAsync(aIntegrationMessage.Content.DiscordRoleIdList, aCancellationToken);
    }
}
