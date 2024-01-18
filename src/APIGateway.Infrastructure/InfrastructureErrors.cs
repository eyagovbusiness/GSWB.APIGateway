using System.Net;
using TGF.Common.ROP.Errors;

namespace APIGateway.Infrastructure
{
    public static class InfrastructureErrors
    {
        public class Identity
        {
            public static HttpError SignOutFailure => new(
            new Error("SignOutFailure",
                "The requested SignOut failed."),
            HttpStatusCode.BadRequest);

            public static HttpError RefreshTokenFailure => new(
            new Error("RefreshTokenFailure",
                "The token refresh failed."),
            HttpStatusCode.InternalServerError);

            public class Claims
            {
                public static HttpError NoApplicationRoleFound => new(
                new Error("Claims.NoApplicationRoleFound",
                    "Could not generate claims for this member because no application role was assigned to the member."),
                HttpStatusCode.BadRequest);
            }

        }

        public class AuthDatabase
        {
            public static HttpError StoringTokenError => new(
            new Error("AuthDatabase.ErrorSavingTokens",
                "Storing the AuthTokens in database failed."),
            HttpStatusCode.InternalServerError);

            public static HttpError AuthTokenNotFound => new(
            new Error("AuthDatabase.AuthTokenNotFound",
                "Could not find any match of the AuthToken in the database."),
            HttpStatusCode.NotFound);

            public static HttpError NotAllTokenRevocationSaved => new(
            new Error("AuthDatabase.NotAllTokenRevocationSaved",
                "One ore more revocation changes were not saved in DB."),
            HttpStatusCode.InternalServerError);

        }

        public class AuthTokenRefreshRequest
        {
            public static HttpError Invalid => new(
            new Error("AuthTokenRefresh.BadTokenRefreshRequest.Invalid",
                "The token requested to refresh is not valid or not recognized by the authDB."),
            HttpStatusCode.BadRequest);

            public static HttpError NotExpired => new(
            new Error("AuthTokenRefresh.BadTokenRefreshRequest.NotExpired",
                "The token sent in this token refresh request has not expired yet."),
            HttpStatusCode.BadRequest);

            public static HttpError ServerError => new(
            new Error("AuthTokenRefresh.ServerError",
                "An error occured in the server side while processing the token refresh request."),
            HttpStatusCode.InternalServerError);

        }

        public class Discord
        {
            public static HttpError MemberRoleFetchNotFound => new(
            new Error("MemberRoleFetch.NotFound",
                "Could not find any member with the provided Id in the discord guil or the member found does not have assigned any role yet."),
            HttpStatusCode.NotFound);
            public static HttpError MemberRoleFetchError => new(
            new Error("MemberRoleFetch.InternalServerError",
                "The service was unable to fetch information about the member related with the provided Id and his assigned roles."),
            HttpStatusCode.InternalServerError);
        }
    }
}
