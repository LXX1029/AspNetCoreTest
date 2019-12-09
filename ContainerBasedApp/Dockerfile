#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-nanoserver-1803 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-nanoserver-1803 AS build
WORKDIR /src
COPY ["ContainerBasedApp/ContainerBasedApp.csproj", "ContainerBasedApp/"]
RUN dotnet restore "ContainerBasedApp/ContainerBasedApp.csproj"
COPY . .
WORKDIR "/src/ContainerBasedApp"
RUN dotnet build "ContainerBasedApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ContainerBasedApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ContainerBasedApp.dll"]