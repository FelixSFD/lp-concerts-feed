namespace Server.Api.Config;

internal static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// Returns the private key for the Apple Music API
        /// </summary>
        /// <returns></returns>
        public string? GetAppleMusicPrivateKey()
        {
            return configuration.GetValue<string>("AppleMusic:PrivateKey");
        }
        
        /// <summary>
        /// Returns the team ID for the Apple Music API
        /// </summary>
        /// <returns></returns>
        public string? GetAppleMusicTeamId()
        {
            return configuration.GetValue<string>("AppleMusic:TeamId");
        }
        
        /// <summary>
        /// Returns the key ID for the Apple Music API
        /// </summary>
        /// <returns></returns>
        public string? GetAppleMusicKeyId()
        {
            return configuration.GetValue<string>("AppleMusic:KeyId");
        }
    }
}