using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Amazon.SimpleNotificationService;
using Moq;
using Nuages.Identity.AWS.Sender;
using Nuages.Identity.Services.Email.Sender;
using Xunit;

namespace Nuages.Identity.Services.Tests.AWS;

public class TestsAWSSender
{
    [Fact]
    public async Task ShouldSendEmailWithSuccess()
    {
        var emailService = new Mock<IAmazonSimpleEmailServiceV2>();
        var notificationService = new Mock<IAmazonSimpleNotificationService>();

        var sender = new AWSSender(emailService.Object, notificationService.Object);

        sender.SendEmailUsingTemplateAsync("sys@nnuages.org", "test@nuages.org", "", "fr",
            new Dictionary<string, string>());
    }
    
    [Fact]
    public async Task ShouldFailEmailWithSuccessCallBase()
    {
        var emailService = new Mock<IAmazonSimpleEmailServiceV2>();
        var notificationService = new Mock<IAmazonSimpleNotificationService>();

        emailService.Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequest>(), new CancellationToken())).Callback(() => throw new NotFoundException(""));
        
        var sender = new AWSSender(emailService.Object, notificationService.Object);

        sender.SendEmailUsingTemplateAsync("sys@nnuages.org", "test@nuages.org", "", "fr",
            new Dictionary<string, string>());
    }
}