FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base

WORKDIR /app 
EXPOSE 5000

# Add some libs required by .NET runtime 
# RUN apk add --no-cache libstdc++ libintl icu-libs

# Runtime configuration options for globalization
# https://docs.microsoft.com/en-us/dotnet/core/runtime-config/globalization
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT false
ENV DOTNET_RUNNING_IN_CONTAINER 1
ENV ASPNETCORE_URLS=http://+:5000

# RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY . .

# Fix dotnet restore
# RUN curl -o /usr/local/share/ca-certificates/verisign.crt -SsL https://crt.sh/?d=1039083 && update-ca-certificates

# RUN dotnet publish
RUN cd src/Example.EntityFramework.Tree && dotnet restore && dotnet publish -c Release -o /app/out \
    --runtime linux-x64 \
    --no-self-contained

# FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime
FROM base as final
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "Example.EntityFramework.Tree.dll"]