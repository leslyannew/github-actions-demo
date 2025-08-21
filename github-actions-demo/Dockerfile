#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# chiseled image will remove pacakage manager and other tools (for production)
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy-chiseled AS base-prod
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

#BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["github_actions_demo.csproj", "github_actions_demo/"]
#COPY ["NuGet.Config", "github_actions_demo/"]

#Generate self-signed certificates
#RUN /Certificates/create-certificate.sh -n github_actions_demo -p 1234 -c  
#COPY /Certificates/* /https/

RUN dotnet restore "github_actions_demo//github_actions_demo.csproj" --disable-parallel 
WORKDIR "/src/github_actions_demo"
COPY . .
RUN dotnet build "github_actions_demo.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "github_actions_demo.csproj"  -a $TARGETARCH  -c $BUILD_CONFIGURATION  --self-contained true -o /app/publish 

#SERVE
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#COPY --from=build /https/* /usr/local/share/ca-certificates
#RUN update-ca-certificates 
USER $APP_UID
HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail --insecure -I https://localhost:8443/hc/ready || exit 1
ENTRYPOINT ["./github_actions_demo"]

FROM base-prod AS final-prod
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /https/* /usr/local/share/ca-certificates
RUN update-ca-certificates 
USER $APP_UID
HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail --insecure -I https://localhost:8443/hc/ready || exit 1
ENTRYPOINT ["./github_actions_demo"]





