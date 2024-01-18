using TGF.CA.Infrastructure.DB.PostgreSQL;

namespace APIGateway.Infrastructure
{
    /// <summary>
    /// Provides a factory for creating instances of <see cref="ApiGatewayAuthDbContext"/> during design time.
    /// This is used primarily for Entity Framework migrations and other design-time operations.
    /// </summary>
    /// <remarks>
    /// WARNING: This factory should ONLY be used in a development environment for design-time operations.
    /// Production or Staging connection strings will never be used for design-time operations. 
    /// </remarks>
    public class AuthorizationDbContextFactory : PostgreSQLDesignTimeDbContextFactory<APIGatewayAuthDbContext>
    {
        protected override string GetSchemaName() => "security";
    }

}
