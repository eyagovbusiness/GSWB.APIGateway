﻿using Common.Application.DTOs.Auth;
using Common.Application.DTOs.Members;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Application
{
    public interface IMembersCommunicationService
    {
        Task<IHttpResult<MemberDTO>> GetExistingMember(ulong aDiscordUserId, CancellationToken aCancellationToken = default);
        Task<IHttpResult<MemberDetailDTO>> SignUpNewMember(SignUpDataDTO? aSignUpDataDTO, DiscordCookieUserInfo aDiscordCookieUserInfo, CancellationToken aCancellationToken = default);

    }
}
