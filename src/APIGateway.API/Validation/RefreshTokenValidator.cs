using APIGateway.Application.DTOs;
using APIGateway.Infrastructure.Helpers.Token;
using FluentValidation;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace APIGateway.API.Validation
{
    public class RefreshTokenValidator : AbstractValidator<TokenPairDTO>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .Length(TokenGenerationHelpers.RefreshTokenLength).WithErrorCode(PresentationErrors.Validation.RefreshToken.InvalidLenght_Code);
        }
    }
}
