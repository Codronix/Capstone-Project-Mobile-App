using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Kerlyn_Liner_App.Services
{
    class EmailService
    {
        public async void SendMailMessage(string RecipientEmail, string MailMessage)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("<Name of email you want to use>", "<Email to send verification codes>"));
            message.To.Add(MailboxAddress.Parse(RecipientEmail));
            message.Subject = "Email Verification";
            message.Body = new TextPart("plain")
            {
                Text = MailMessage
            };
            string SystemEmailAddress = "<Email to send verification codes>";
            string SystemEmailPassword = "<Password of email>";
            SmtpClient client = new SmtpClient();
            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(SystemEmailAddress, SystemEmailPassword);
                client.Send(message);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }
}
