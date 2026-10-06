FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY . .

RUN dotnet restore PaySuite.Sdk.slnx

RUN dotnet build PaySuite.Sdk.slnx --no-restore

CMD ["dotnet", "test", "PaySuite.Sdk.slnx", "--no-build"]
