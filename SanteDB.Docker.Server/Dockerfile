# SanteDB Server Dockerfile  
FROM mono
MAINTAINER "SanteSuite Contributors"
RUN mkdir /santedb
COPY ./bin/Debug /santedb
WORKDIR /santedb
EXPOSE 2100/tcp
EXPOSE 8080/tcp
CMD [ "mono", "/santedb/SanteDB.Docker.Server.exe" ]