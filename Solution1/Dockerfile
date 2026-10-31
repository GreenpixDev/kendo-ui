# escape=`
# Windows-контейнеры: .NET Framework 4.8 и ASP.NET MVC 5 в Linux-контейнерах не работают.

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS build
WORKDIR C:\src
COPY Reports.Web Reports.Web
RUN msbuild Reports.Web\Reports.Web.csproj -restore /p:Configuration=Release /nologo -v:minimal

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022
WORKDIR C:\inetpub\wwwroot
COPY --from=build C:\src\Reports.Web\bin bin
COPY --from=build C:\src\Reports.Web\Views Views
COPY --from=build C:\src\Reports.Web\Scripts Scripts
COPY --from=build C:\src\Reports.Web\Content Content
COPY --from=build C:\src\Reports.Web\Web.config Web.config
COPY --from=build C:\src\Reports.Web\Global.asax Global.asax
