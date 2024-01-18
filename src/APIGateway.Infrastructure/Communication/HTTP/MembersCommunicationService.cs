using APIGateway.Application;
using Common.Application.Contracts.ApiRoutes;
using Common.Application.DTOs;
using Common.Infrastructure;
using TGF.CA.Infrastructure.Communication.Http;
using TGF.CA.Infrastructure.Discovery;
using TGF.Common.ROP.HttpResult;


namespace APIGateway.Infrastructure.Communication.HTTP
{
    /// <summary>
    /// Provides services for communicating with the Members API.
    /// </summary>
    public class MembersCommunicationService : HttpCommunicationService, IMembersCommunicationService
    {
        private readonly string _serviceName;

        public MembersCommunicationService(IServiceDiscovery aServiceDiscovery, IHttpClientFactory aHttpClientFactory)
        : base(aServiceDiscovery, aHttpClientFactory)
            => _serviceName = ServicesDiscoveryNames.Members;

        public async Task<IHttpResult<MemberDTO>> GetExistingMember(ulong aDiscordUserId, CancellationToken aCancellationToken = default)
            => await GetAsync<MemberDTO>(_serviceName, $"/{MembersApiRoutes.private_members_getByDiscordUserId}?aDiscordUserId={aDiscordUserId}", aCancellationToken);

        public async Task<IHttpResult<MemberDetailDTO>> SignUpNewMember(SignUpDataDTO? aSignUpDataDTO, DiscordCookieUserInfo aDiscordCookieUserInfo, CancellationToken aCancellationToken = default)
            => await PutAsync<CreateMemberDTO, MemberDetailDTO>(_serviceName, $"/{MembersApiRoutes.private_members_addNew}", new CreateMemberDTO(aSignUpDataDTO, aDiscordCookieUserInfo), aCancellationToken);

    }
}
