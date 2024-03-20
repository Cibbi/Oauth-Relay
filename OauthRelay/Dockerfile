# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk update && apk add clang build-base zlib-dev
WORKDIR /src
COPY ["GithubOauthRelay/GithubOauthRelay.csproj", "GithubOauthRelay/"]
RUN dotnet restore "GithubOauthRelay/GithubOauthRelay.csproj"
COPY . .
WORKDIR "/src/GithubOauthRelay"

RUN dotnet publish "GithubOauthRelay.csproj" -c Release -r linux-musl-x64 -o /app/publish

# Final Stage
FROM alpine:latest AS final
WORKDIR /app
COPY --from=build /app/publish/GithubOauthRelay ./GithubOauthRelay
COPY --from=build /app/publish/appsettings.json ./appsettings.json
ENTRYPOINT ["./GithubOauthRelay"]

# Expose ports
EXPOSE 8080
EXPOSE 8081