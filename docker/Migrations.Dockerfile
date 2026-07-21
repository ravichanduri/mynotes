FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src
COPY . .
RUN dotnet restore src/NotesHub.Api/NotesHub.Api.csproj --configfile NuGet.Config \
    && dotnet build src/NotesHub.Api/NotesHub.Api.csproj -c Release --no-restore \
    && dotnet tool install --tool-path /tools dotnet-ef --version 8.0.29
ENTRYPOINT ["/tools/dotnet-ef", "database", "update", "--configuration", "Release", "--no-build", "--project", "src/NotesHub.Infrastructure/NotesHub.Infrastructure.csproj", "--startup-project", "src/NotesHub.Api/NotesHub.Api.csproj"]
