using CustomOverlayFramework.Enums;
using CustomOverlayFramework.Utils;
using Exiled.API.Features;
using System.Net;

namespace CustomOverlayFramework.Authentification
{
    public class AuthentificationHandler
    {
        private readonly Config _config;
        public AuthentificationHandler(Config config)
        {
            _config = config;
        }

        public AuthenticationResponse Authenticate(string line, IPAddress ip, out string userId)
        {
            userId = null;
            if (string.IsNullOrWhiteSpace(line))
            {
                return AuthenticationResponse.Refused;
            }

            string[] parts = line.Split(' ');
            if(parts.Length != 2 || parts[0] != "AUTH")
            {
                return AuthenticationResponse.Refused;
            }

            userId = parts[1];
            return GetResponse(userId, ip);
        }

        private AuthenticationResponse GetResponse(string userId, IPAddress ip)
        {
            if (IsPlayerBlacklisted(userId)) {
                return AuthenticationResponse.Banned;
            }

            Player player = Player.Get(userId);
            if (player == null)
            {
                return AuthenticationResponse.Refused;
            }

            if(HasSameIp(player, ip))
            {
                return AuthenticationResponse.Welcome;   
            }

            return AuthenticationResponse.Refused;
        }

        private bool IsPlayerBlacklisted(string userId)
        {
            return _config.BlacklistedUserId.Contains(userId);
        }

        private bool HasSameIp(Player player, IPAddress ip)
        {
            if (!IPAddress.TryParse(player.IPAddress, out IPAddress playerIp))
            {
                return false;
            }

            playerIp = IPUtils.Normalize(playerIp);
            ip = IPUtils.Normalize(ip);

            return playerIp.Equals(ip);
        }
    }
}
