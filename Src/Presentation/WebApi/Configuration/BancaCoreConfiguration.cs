using Microsoft.OpenApi.Models;

namespace BancaCore.WebApi.Configuration
{
    public class BancaCoreConfiguration
    {
        public string ApiName { get; set; }

        public string ApiVersion { get; set; }
        public string ApiDescription { get; set; }

        public string ApiBaseUrl { get; set; }

        public string OidcSwaggerUIClientId { get; set; }

        public bool RequireHttpsMetadata { get; set; }

        public string OidcApiName { get; set; }

        public bool CorsAllowAnyOrigin { get; set; }

        public string[] CorsAllowOrigins { get; set; }
        public string Website { get; set; }

        public OpenApiContact OpenApiContact { get; set; }
    }
}
