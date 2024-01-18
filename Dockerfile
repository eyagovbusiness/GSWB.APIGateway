FROM gswb.common:latest AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build 
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Copy NuGet packages and project files from the base image
COPY --from=base /app/BasePackages ./BasePackages
COPY projectfiles.tar .

# Restore dependencies in a docker cache friendly way
RUN tar -xvf projectfiles.tar \
    && dotnet restore "src/APIGateway.API/APIGateway.API.csproj" --configfile ./NuGet.config /p:DockerBuild=true \
    && rm projectfiles.tar  # Remove the tar file to reduce image size

COPY . .
RUN dotnet build "src/APIGateway.API/APIGateway.API.csproj" -c $BUILD_CONFIGURATION --no-restore /p:DockerBuild=true

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/APIGateway.API/APIGateway.API.csproj" -c $BUILD_CONFIGURATION --no-build -o /app/publish /p:UseAppHost=false
#RUN dotnet publish "src/APIGateway.API/APIGateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:DockerBuild=true /p:PublishReadyToRun=true /p:PublishSingleFile=true /p:UseAppHost=true /p:RuntimeIdentifier=linux-musl-x64

FROM base AS final 
WORKDIR /app
COPY --from=publish /app/publish .
COPY Infrastructure/APIGatewayEntrypointOverride.sh ./entrypoint.sh
COPY Infrastructure/ServiceAwait/wait_for_service.sh ./wait_for_service.sh
COPY Infrastructure/ServiceAwait/IsReadyServer.sh ./IsReadyServer.sh
#RUN chmod +x ./IsReadyServer.sh
ENTRYPOINT ["sh", "entrypoint.sh"]
#ENTRYPOINT ["dotnet", "APIGateway.API.dll"]
