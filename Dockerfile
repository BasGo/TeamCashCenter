FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy only project files for restore
COPY *.sln ./
# project file is at repo root
COPY TeamCashCenter.csproj ./
RUN dotnet restore "TeamCashCenter.csproj" --runtime linux-x64

# copy remaining sources and publish for linux-x64 with trimming
COPY . .
WORKDIR /src
RUN dotnet publish "TeamCashCenter.csproj" -c Release -r linux-x64 -o /app/publish --self-contained false /p:PublishTrimmed=true /p:TrimMode=link --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:80

# Recommended runtime configuration for SMTP — set these as environment variables
# do NOT bake secrets into the image. Example environment variables to set for the app:
#   Email__Host=smtp.example.com
#   Email__Port=587
#   Email__EnableSsl=true
#   Email__Username=smtp-user@example.com
#   Email__Password=supersecret
#   Email__From=no-reply@example.com
#   Email__AdminContact=admin@example.com
#   Email__ResetTokenExpiryHours=24

# add minimal tooling for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates \
	&& rm -rf /var/lib/apt/lists/*

# create non-root user with fixed UID/GID to avoid file permission surprises
RUN groupadd -g 1000 appgroup || true && useradd -m -u 1000 -g appgroup appuser || true
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80

USER appuser
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl -f http://localhost/ready || exit 1
ENTRYPOINT ["dotnet", "TeamCashCenter.dll"]
