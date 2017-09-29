using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;

public static async Task<HttpResponseMessage> Run( HttpRequestMessage req, TraceWriter log )
{
    log.Info( $"Recieved marketing call: {req.RequestUri}" );
    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();
    // Set name to query string or body data
    var email = data?.email ?? string.Empty;
    var city = data?.city ?? string.Empty;
    log.Info( $"Request received from {email}" );

    // Send the 'request recieved email' 
    dynamic sg = new SendGridAPIClient( "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" );
    // Add the message properties.
    var fromEmail = new Email( "marketing@bikesharing360.com" );
    var toEmail = new Email( email.ToString() );
    var html = $"<h1>Your request has been received</h1><p>We have recieved your request for information." +
    $"When <i>BikeSharing360<i> would we available in {city} you will be notified</p>" +
    $"<p>Stay tunned!</p>";
    var content = new Content( "text/html", html );
    var mail = new Mail( fromEmail, "Request received!", toEmail, content );
    var response = await sg.client.mail.send.post( requestBody: mail.Get() );
    var responseMessage = response.Body.ReadAsStringAsync().Result;
    var statusCode = (System.Net.HttpStatusCode)response.StatusCode;
    log.Info( $"Response status {statusCode} - {responseMessage}" );
    return req.CreateResponse( statusCode, "" );
}
