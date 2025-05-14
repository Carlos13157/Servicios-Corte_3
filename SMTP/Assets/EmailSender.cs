using System;
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EmailSender : MonoBehaviour {

    public string fromEmail = "";
    public string password = "";
    public List<string> toEmails = new();

    public void SendEmail() {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(fromEmail);

        // Agrega todos los correos de la lista
        foreach (string email in toEmails) {
            mail.To.Add(email);
        }

        mail.Subject = "Spam";
        mail.Body = "Este correo es Spam desde un proyecto en Unity, Atte: Carlos";

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential(fromEmail, password) as ICredentialsByHost;
        smtpServer.EnableSsl = true;

        try {
            smtpServer.Send(mail);
            Debug.Log("Correo enviado exitosamente");
        }
        catch (System.Exception e) {
            Debug.LogError("Error al enviar el correo: " + e.Message);
        }
    }


    public void SendSpam() {

    }
}
