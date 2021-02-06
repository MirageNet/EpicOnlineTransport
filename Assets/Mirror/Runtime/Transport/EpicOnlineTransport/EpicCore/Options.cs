#region Statements

using System;

#endregion

namespace Epic.Core
{
    [Serializable]
    public struct Options
    {
        public string ProductId;
        public string SandboxId;
        public string DeploymentId;
        public string ClientId;
        public string ClientSecret;
        public string ProductName;
        public string ProductVersion;
    }
}
