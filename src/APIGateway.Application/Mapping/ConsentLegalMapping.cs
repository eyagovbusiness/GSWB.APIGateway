using APIGateway.Domain.Entities;
using Common.Application.DTOs.Legal;

namespace APIGateway.Application.Mapping
{
    public static class ConsentLegalMapping
    {
        public static ConsentLog ToEntity(this ConsentLogDTO aConsentLogDto, string aUserIpAddress)
        => new() 
        { 
            ConsentDate = DateTimeOffset.Now,
            ConsentMethod = aConsentLogDto.ConsentMethod,
            ConsentType = aConsentLogDto.ConsentType,
            PrivacyPolicyVersion = aConsentLogDto.PrivacyPolicyVersion,
            IpAddress = aUserIpAddress,
            Geolocation = aConsentLogDto.Geolocation,
            UserAgent = aConsentLogDto.UserAgent
        };
    }
}
