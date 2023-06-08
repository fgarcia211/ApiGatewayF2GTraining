using ModelsF2GTraining;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ApiGatewayF2GTraining.Helpers
{
    public static class HelperContextUser
    {
        public static Usuario GetUsuarioByClaim(Claim claim)
        {
            return JsonConvert.DeserializeObject<Usuario>(claim.Value);
        }
    }
}
