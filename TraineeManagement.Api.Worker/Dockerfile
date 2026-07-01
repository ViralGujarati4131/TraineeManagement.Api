FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY TraineeManagement.Api.Data/*.csproj TraineeManagement.Api.Data/
COPY TraineeManagement.Api.Messaging/*.csproj TraineeManagement.Api.Messaging/
COPY TraineeManagement.Api.Contract/*.csproj TraineeManagement.Api.Contract/
COPY TraineeManagement.Api.Worker/*.csproj TraineeManagement.Api.Worker/

RUN dotnet restore TraineeManagement.Api.Worker/TraineeManagement.Api.Worker.csproj

COPY . .
RUN dotnet publish TraineeManagement.Api.Worker/TraineeManagement.Api.Worker.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "TraineeManagement.Api.Worker.dll"]