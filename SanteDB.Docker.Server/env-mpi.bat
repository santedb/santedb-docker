﻿@ECHO OFF


IF [%1] == []  (
	SET DB_NAME=santedb
) ELSE (
	SET DB_NAME=%1
)

REM SET THE DOCKER FLAGS TO SANTEMPI DEFAULTS (NOTE YOU MAY HAVE TO COPY THE SANTEMPI.*.DLL FILES)
SET SDB_FEATURE=LOG;DATA_POLICY;AUDIT_REPO;ADO;RAMCACHE;SEC;SWAGGER;OPENID;MDM;FHIR;HL7;HDSI;AMI;BIS;MATCHING;PUBSUB_ADO;IHE_PDQM;IHE_PIXM;IHE_PMIR
SET SDB_LOG_LEVEL=Informational
SET SDB_MATCHING_MODE=WEIGHTED
SET SDB_MDM_RESOURCE=Patient=org.santedb.matching.patient.default
SET SDB_MDM_AUTO_MERGE=false
SET SDB_DB_MAIN=server=sdb-postgres;port=5432; database=%DB_NAME%; user id=postgres; password=postgres; pooling=true; MinPoolSize=5; MaxPoolSize=15; Timeout=60;
SET SDB_DB_AUDIT=server=sdb-postgres;port=5432; database=%DB_NAME%_audit; user id=postgres; password=postgres; pooling=true; MinPoolSize=5; MaxPoolSize=15; Timeout=60;
SET SDB_DB_MAIN_PROVIDER=Npgsql
SET SDB_DB_AUDIT_PROVIDER=Npgsql
SET SDB_FHIR_BASE=http://dilbert.com/fhir
SET SDB_DELAY_START=1000

SANTEDB.DOCKER.SERVER.EXE