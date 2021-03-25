# SanteDB Server Dockerfile  
FROM mono
MAINTAINER "SanteSuite Contributors"
RUN mkdir /santedb
RUN mkdir /santedb/data
COPY ./bin/Release/ /santedb/
WORKDIR /santedb
EXPOSE 2100/tcp
EXPOSE 8080/tcp
ENV SDB_FEATURE=RAMCACHE;ADO;PUBSUB_ADO;SEC;LOG;FHIR;HL7;HDSI;AMI;BIS;SWAGGER;AUDIT_REPO;OPENID
CMD [ "mono", "/santedb/SanteDB.Docker.Server.exe" ]