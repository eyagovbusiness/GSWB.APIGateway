using APIGateway.Application.DTOs;
using APIGateway.Domain.Entities;

namespace APIGateway.Application.Mapping
{
    public static class TokenPairAuthRecordMapping
    {
        public static TokenPairDTO ToDto(this TokenPairAuthRecord aTokenPairAuthRecord)
            => new(aTokenPairAuthRecord.AccessToken, aTokenPairAuthRecord.RefreshToken);
    }
}
