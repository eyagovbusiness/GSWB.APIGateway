#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/APIGateway/APIGateway.API/APIGateway.API.csproj", "src/APIGateway/APIGateway.API/"]
COPY ["src/Common/Common.Presentation/Common.Presentation.csproj", "src/Common/Common.Presentation/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Presentation/TGF.CA.Presentation/TGF.CA.Presentation.csproj", "TheGoodFramework/src/CleanArchitecture/Presentation/TGF.CA.Presentation/"]
COPY ["TheGoodFramework/src/Common/TGF.Common.Logging/TGF.Common.Logging.csproj", "TheGoodFramework/src/Common/TGF.Common.Logging/"]
COPY ["TheGoodFramework/src/Common/TGF.Common.ROP/TGF.Common.ROP.csproj", "TheGoodFramework/src/Common/TGF.Common.ROP/"]
COPY ["TheGoodFramework/src/Common/TGF.Common.Serialization/TGF.Common.Serialization.csproj", "TheGoodFramework/src/Common/TGF.Common.Serialization/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/TGF.CA.Infrastructure/TGF.CA.Infrastructure.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/TGF.CA.Infrastructure/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Application/TGF.CA.Application/TGF.CA.Application.csproj", "TheGoodFramework/src/CleanArchitecture/Application/TGF.CA.Application/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Domain/TGF.CA.Domain/TGF.CA.Domain.csproj", "TheGoodFramework/src/CleanArchitecture/Domain/TGF.CA.Domain/"]
COPY ["src/APIGateway/APIGateway.Infrastructure/APIGateway.Infrastructure.csproj", "src/APIGateway/APIGateway.Infrastructure/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/Communication/TGF.CA.Infrastructure.Communication.RabbitMQ/TGF.CA.Infrastructure.Communication.RabbitMQ.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/Communication/TGF.CA.Infrastructure.Communication.RabbitMQ/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/TGF.CA.Infrastructure.Discovery/TGF.CA.Infrastructure.Discovery.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/TGF.CA.Infrastructure.Discovery/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/Communication/TGF.CA.Infrastructure.Communication/TGF.CA.Infrastructure.Communication.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/Communication/TGF.CA.Infrastructure.Communication/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/Persistence/DB/TGF.CA.Infrastructure.DB.PostgreSQL/TGF.CA.Infrastructure.DB.PostgreSQL.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/Persistence/DB/TGF.CA.Infrastructure.DB.PostgreSQL/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/Persistence/DB/TGF.CA.Infrastructure.DB/TGF.CA.Infrastructure.DB.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/Persistence/DB/TGF.CA.Infrastructure.DB/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/Security/TGF.CA.Infrastructure.Identity/TGF.CA.Infrastructure.Identity.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/Security/TGF.CA.Infrastructure.Identity/"]
COPY ["TheGoodFramework/src/CleanArchitecture/Infrastructure/Security/TGF.CA.Infrastructure.Secrets/TGF.CA.Infrastructure.Secrets.csproj", "TheGoodFramework/src/CleanArchitecture/Infrastructure/Security/TGF.CA.Infrastructure.Secrets/"]
COPY ["src/Common/Common.Infrastructure/Common.Infrastructure.csproj", "src/Common/Common.Infrastructure/"]
COPY ["Mandril/src/Mandril.Infrastructure/Mandril.Infrastructure.csproj", "Mandril/src/Mandril.Infrastructure/"]
COPY ["Mandril/src/Mandril.Application/Mandril.Application.csproj", "Mandril/src/Mandril.Application/"]
COPY ["Mandril/src/Mandril.Domain/Mandril.Domain.csproj", "Mandril/src/Mandril.Domain/"]
COPY ["Mandril/src/MandrilBot/MandrilBot.csproj", "Mandril/src/MandrilBot/"]
COPY ["TheGoodFramework/src/Common/TGF.Common.Extensions/TGF.Common.Extensions.csproj", "TheGoodFramework/src/Common/TGF.Common.Extensions/"]
COPY ["TheGoodFramework/src/Common/TGF.Common.Net/TGF.Common.Net.csproj", "TheGoodFramework/src/Common/TGF.Common.Net/"]
COPY ["src/Common/Common.Application/Common.Application.csproj", "src/Common/Common.Application/"]
COPY ["src/Common/Common.Domain/Common.Domain.csproj", "src/Common/Common.Domain/"]
COPY ["src/APIGateway/APIGateway.Application/APIGateway.Application.csproj", "src/APIGateway/APIGateway.Application/"]
COPY ["src/APIGateway/APIGateway.Domain/APIGateway.Domain.csproj", "src/APIGateway/APIGateway.Domain/"]
RUN dotnet restore "./src/APIGateway/APIGateway.API/./APIGateway.API.csproj"
COPY . .
WORKDIR "/src/src/APIGateway/APIGateway.API"
RUN dotnet build "./APIGateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./APIGateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Infrastructure/MicroserviceEntrypointOverrides/APIGatewayEntrypointOverride.sh ./entrypoint.sh
COPY Infrastructure/ServiceAwait/wait_for_service.sh ./wait_for_service.sh
ENTRYPOINT ["sh", "entrypoint.sh"]