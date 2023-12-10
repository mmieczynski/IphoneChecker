using Serilog;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Iphone15;

public class IphoneChecker(HttpClient client) : BackgroundService
{
    private const int Delay = 5 * 60 * 1000; // 5 minutes
    private const int IterationsAfter24Hours = 60 / 5 * 24; // 60 mins / 5 mins * 24 hours
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Starting iphoner service!");
        const string accountSid = "AC15105eeccd558f6a2820d62a128589e2";
        const string authToken = "95bcecdb24cb8fa85e3667b9a65a1e85";
        TwilioClient.Init(accountSid, authToken);
        
        await Task.Delay(500, stoppingToken);
        await SendSms("Starting the service");

        var iteration = 0;
        while (true)
        {
            await Task.Delay(Delay, stoppingToken);

            iteration++;
            if (iteration >= IterationsAfter24Hours)
            {
                await SendSms("Still looking...");
                iteration = 0;
            }
            
            Log.Information("Checking for changes on the site.");
            var response = await client.GetAsync(new Uri("https://www.orange.pl/esklep/smartfony/apple/iphone-15-pro-max-512gb-5g"), stoppingToken);
            var html = await response.Content.ReadAsStringAsync(stoppingToken);
            var checks = new List<string>
            {
                """
                class="hidden" checked="" value="Tytanowy czarny"
                """,
                """
                class="hidden" value="Tytanowy naturalny"
                """
            };
            
            if (checks.All(check => html.Contains(check))) continue;
            
            Log.Information("Change found! Sending SMS.");
            await SendSms("Iphone is available!");

            Log.Information("Message sent.");
            
            return;
        }
    }

    private static async Task SendSms(string body)
    {
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber("+48784538039"))
        {
            From = new PhoneNumber("+12027653838"),
            Body = body
        };
        await MessageResource.CreateAsync(messageOptions);
    }
}