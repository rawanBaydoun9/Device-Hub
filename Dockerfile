FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["Task2/Task2.csproj", "Task2/"]
RUN dotnet restore "Task2/Task2.csproj"

COPY . .

WORKDIR "/src/Task2"
RUN dotnet publish "Task2.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Task2.dll"]