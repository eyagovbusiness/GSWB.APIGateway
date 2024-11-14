using Common.Application.Contracts.Services;
using Common.Application.DTOs.Guilds;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TGF.CA.Application.UseCases;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Application.UseCases
{
    public class ListUserGuilds(ISwarmBotCommunicationService swarmBotCommunicationService)
        : IUseCase<IHttpResult<IEnumerable<GuildDTO>>, ClaimsPrincipal>
    {
        public async Task<IHttpResult<IEnumerable<GuildDTO>>> ExecuteAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
        {
            return await swarmBotCommunicationService.GetUserGuildList(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!, cancellationToken);
        }
    }
}
