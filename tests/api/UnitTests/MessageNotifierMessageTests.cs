using CivicConnect.Notifications;
using CivicConnect.Requests;

using MimeKit;

namespace CivicConnect.UnitTests;

public class EmailNotifierMessageTests
{
    [Fact(DisplayName = "S2-09 / FR-006: email carries the request id and new status and is addressed to the requester")]
    public void Message_has_request_id_new_status_and_recipient()
    {
        var settings = new EmailOptions { FromAddress = "noreply@example.com", FromName = "CivicConnect" };
        var notification = new StatusChangeNotification(5, 42, "In Progress", "Ada Lovelace", "ada@example.com");

        var message = EmailNotifier.BuildMessage(notification, settings);

        var to = Assert.IsType<MailboxAddress>(Assert.Single(message.To));
        Assert.Equal("ada@example.com", to.Address);
        Assert.Contains("42", message.Subject);
        Assert.Contains("In Progress", message.Subject);
        Assert.Contains("Request ID: 42", message.TextBody);
        Assert.Contains("New status: In Progress", message.TextBody);
    }

    [Fact(DisplayName = "S2-09 / FR-006: email states that replying cannot change the request")]
    public void Message_does_not_offer_a_way_to_modify_the_request()
    {
        var notification = new StatusChangeNotification(5, 42, "Assigned", "Ada Lovelace", "ada@example.com");

        var message = EmailNotifier.BuildMessage(notification, new EmailOptions { FromAddress = "noreply@example.com" });

        Assert.Contains("will not change your request", message.TextBody);
    }
}