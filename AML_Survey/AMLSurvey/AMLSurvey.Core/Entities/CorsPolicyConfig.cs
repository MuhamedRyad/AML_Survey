namespace AMLSurvey.Core.Entities
{
    public class CorsPolicyConfig
    {
        public string PolicyName { get; set; } = "DefaultPolicy";
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; } = false;
    }
}
