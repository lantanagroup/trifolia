FROM microsoft/aspnet

COPY ["./Trifolia.Web/obj/Install Release/Package/PackageTmp", "/inetpub/wwwroot"]