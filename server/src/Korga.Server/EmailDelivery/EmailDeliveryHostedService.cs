using Korga.EmailDelivery.Entities;
using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailDelivery;

public class EmailDeliveryHostedService : RepeatedExecutionService
{
    private readonly IOptions<EmailDeliveryOptions> options;
    private readonly IServiceProvider serviceProvider;

    public EmailDeliveryHostedService(ILogger<EmailDeliveryHostedService> logger, IOptions<EmailDeliveryOptions> options, IServiceProvider serviceProvider)
        : base(logger)
    {
        this.options = options;
        this.serviceProvider = serviceProvider;

        Interval = TimeSpan.FromMinutes(options.Value.DeliveryIntervalInMinutes);
    }

    protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
        DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        SmtpDeliveryService smtpDelivery = serviceScope.ServiceProvider.GetRequiredService<SmtpDeliveryService>();

        for (int i = 0; i < options.Value.BatchSize || options.Value.BatchSize == 0; i++)
        {
            OutboxEmail? email = await database.OutboxEmails
                .FirstOrDefaultAsync(email => email.DeliveryTime == default, stoppingToken);

            if (email == null) break;

            bool sentSuccessfully = await smtpDelivery.Send(email, stoppingToken);
            if (!sentSuccessfully) break;
        }
    }
}
