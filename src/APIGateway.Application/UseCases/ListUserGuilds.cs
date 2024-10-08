using Common.Application.Contracts.Services;
using Common.Application.DTOs.Guilds;
using TGF.CA.Application.UseCases;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Application.UseCases
{
    public class ListUserGuilds(ISwarmBotCommunicationService swarmBotCommunicationService)
        : IUseCase<IHttpResult<IEnumerable<GuildDTO>>, string>
    {
        public async Task<IHttpResult<IEnumerable<GuildDTO>>> ExecuteAsync(string cookieHeader, CancellationToken cancellationToken = default)
        {
            return await swarmBotCommunicationService.GetUserGuildList(cookieHeader, cancellationToken);
        }
    }
}
