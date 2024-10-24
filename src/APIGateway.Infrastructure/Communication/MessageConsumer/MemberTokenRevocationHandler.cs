using APIGateway.Application;
using Common.Application.Contracts.Communication.Messages;
using TGF.CA.Infrastructure.Communication.Consumer.Handler;
using TGF.CA.Infrastructure.Communication.Messages;

namespace APIGateway.Infrastructure.Communication.MessageConsumer
{
    public class MemberTokenRevocationHandler(ITokenRevocationService aTokenRevocationService) 
        : IIntegrationMessageHandler<MemberTokenRevoked>
    {
        private readonly ITokenRevocationService _tokenRevocationService = aTokenRevocationService;

        public async Task Handle(IntegrationMessage<MemberTokenRevoked> aIntegrationMessage, CancellationToken aCancellationToken = default)
            => await _tokenRevocationService.OutdateByDiscordUserListAsync(aIntegrationMessage.Content.MemberIdList.ToArray(), aCancellationToken);
    }
}
