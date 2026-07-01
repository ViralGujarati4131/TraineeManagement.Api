FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY TraineeManagement.Api.Data/*.csproj TraineeManagement.Api.Data/
COPY TraineeManagement.Api/*.csproj TraineeManagement.Api/
COPY TrainingDirectory.Api/*.csproj TrainingDirectory.Api/

RUN dotnet restore TrainingDirectory.Api/TrainingDirectory.Api.csproj

COPY . .
RUN dotnet publish TrainingDirectory.Api/TrainingDirectory.Api.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "TrainingDirectory.Api.dll"]