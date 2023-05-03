# SanteDB Server Dockerfile  
FROM mono:latest
RUN apt-get clean
MAINTAINER "SanteSuite Contributors"
RUN mkdir /santedb
RUN mkdir /santedb/data
COPY ./bin/Release/ /santedb/
RUN rm -vf /santedb/*nunit*
RUN rm -vf /santedb/*NUnit*
# RUN mv -vf /santedb/Data/*.dataset /santedb/data/
# RUN rm -rf /santedb/Data
WORKDIR /santedb
EXPOSE 2100/tcp
EXPOSE 8080/tcp
ENV SDB_FEATURE=RAMCACHE;ADO;PUBSUB_ADO;SEC;LOG;FHIR;HL7;HDSI;AMI;BIS;SWAGGER;AUDIT_REPO;OPENID
CMD [ "mono", "--server", "/santedb/SanteDB.Docker.Server.exe" ]