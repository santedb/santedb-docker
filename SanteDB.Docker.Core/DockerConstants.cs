using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Docker.Core
{
    /// <summary>
    /// Constants for the docker configuration system
    /// </summary>
    public static class DockerConstants
    {

        /// <summary>
        /// Feature lists
        /// </summary>
        public const string EnvFeatureList = "SDB_FEATURE";

        /// <summary>
        /// Environment variables which are passed for configuration
        /// </summary>
        public const string EnvConfigPrefix = "SDB_";

        /// <summary>
        /// Connection string prefix
        /// </summary>
        public const string EnvConnectionStringPrefix = "SDB_DB_";
    }
}
