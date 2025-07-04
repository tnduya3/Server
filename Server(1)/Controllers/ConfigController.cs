using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("firebase")]
        public IActionResult GetFirebaseConfig()
        {
            try
            {
                var firebaseConfig = new
                {
                    apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY") ?? _configuration["Firebase:ApiKey"],
                    authDomain = Environment.GetEnvironmentVariable("FIREBASE_AUTH_DOMAIN") ?? _configuration["Firebase:AuthDomain"],
                    projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") ?? _configuration["Firebase:ProjectId"],
                    storageBucket = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET") ?? _configuration["Firebase:StorageBucket"],
                    messagingSenderId = Environment.GetEnvironmentVariable("FIREBASE_MESSAGING_SENDER_ID") ?? _configuration["Firebase:MessagingSenderId"],
                    appId = Environment.GetEnvironmentVariable("FIREBASE_APP_ID") ?? _configuration["Firebase:AppId"],
                    measurementId = Environment.GetEnvironmentVariable("FIREBASE_MEASUREMENT_ID") ?? _configuration["Firebase:MeasurementId"],
                    vapidKey = Environment.GetEnvironmentVariable("FIREBASE_VAPID_KEY") ?? _configuration["Firebase:VapidKey"]
                };

                return Ok(firebaseConfig);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get Firebase configuration", details = ex.Message });
            }
        }
    }
}
