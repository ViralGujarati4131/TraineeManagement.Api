FROM docker-registry-002.zeuslearning.com/zeuslearning/dotnet/sdk:9.0 AS build
# Replace with the zeus code artificat
WORKDIR /src

COPY nuget.config .
COPY TraineeManagement.Api.Data/*.csproj TraineeManagement.Api.Data/
COPY TraineeManagement.Api.Messaging/*.csproj TraineeManagement.Api.Messaging/
COPY TraineeManagement.Api.Contract/*.csproj TraineeManagement.Api.Contract/
COPY TraineeManagement.Api.Worker/*.csproj TraineeManagement.Api.Worker/

RUN dotnet restore TraineeManagement.Api.Worker/TraineeManagement.Api.Worker.csproj

COPY . .
RUN dotnet publish TraineeManagement.Api.Worker/TraineeManagement.Api.Worker.csproj -c Release -o /app/build

FROM docker-registry-002.zeuslearning.com/zeuslearning/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "TraineeManagement.Api.Worker.dll"]