# SanteDB Docker Containerization

This repository contains the configuration files and plugins for SanteDB to be run in a docker container. 

## Environment Configuration Daemon

The plugin which allows for configuration of the SanteDB instance via environment variables. This plugin works
by allowing multiple `IEnvironmentConfigurationSetting` providers to override the default configuration found 
within the `SanteDBConfiguration` file provided. 