using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
        public static class Templates
        {
            private const string CompanyName = "Gaber Company";
            private const string CompanyYear = "2026";

            public static string WelcomeEmailTemplate(string userName, string activationLink)
            {
                return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #007bff; padding-bottom: 20px; }}
                    .header h1 {{ color: #007bff; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; }}
                    .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome!</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{userName}</strong>,</p>
                        <p>Thank you for registering with us. We're excited to have you on board!</p>
                        <p>Please verify your email address by clicking the button below:</p>
                        <a href='{activationLink}' class='button'>Verify Email Address</a>
                        <p>If you didn't create this account, you can safely ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            }

            public static string PasswordResetTemplate(string userName, string resetLink)
            {
                return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #dc3545; padding-bottom: 20px; }}
                    .header h1 {{ color: #dc3545; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; }}
                    .button {{ display: inline-block; background-color: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                    .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{userName}</strong>,</p>
                        <p>We received a request to reset your password. Click the button below to proceed:</p>
                        <a href='{resetLink}' class='button'>Reset Password</a>
                        <div class='warning'>
                            <p><strong>Note:</strong> This link will expire in 1 hour. If you didn't request this reset, please ignore this email.</p>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            }

            public static string ConfirmationEmailTemplate(string userName, string message)
            {
                return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #28a745; padding-bottom: 20px; }}
                    .header h1 {{ color: #28a745; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; }}
                    .success-box {{ background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>✓ Confirmation</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{userName}</strong>,</p>
                        <div class='success-box'>
                            <p>{message}</p>
                        </div>
                        <p>Thank you for using our service!</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            }

            public static string NotificationEmailTemplate(string userName, string title, string content)
            {
                return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #17a2b8; padding-bottom: 20px; }}
                    .header h1 {{ color: #17a2b8; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; }}
                    .title {{ font-size: 18px; font-weight: bold; color: #17a2b8; margin: 20px 0 10px 0; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Notification</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{userName}</strong>,</p>
                        <div class='title'>{title}</div>
                        <p>{content}</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            }

            public static string OtpEmailTemplate(string userName, string otp, int expirationMinutes = 10)
            {
                return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #ff9800; padding-bottom: 20px; }}
                    .header h1 {{ color: #ff9800; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; }}
                    .otp-box {{ background-color: #fff3e0; border: 2px solid #ff9800; padding: 20px; border-radius: 8px; margin: 20px 0; text-align: center; }}
                    .otp-code {{ font-size: 32px; font-weight: bold; color: #ff9800; letter-spacing: 4px; font-family: 'Courier New', monospace; }}
                    .security-info {{ background-color: #f3f3f3; border-left: 4px solid #ff9800; padding: 15px; margin: 20px 0; font-size: 13px; }}
                    .security-info strong {{ color: #ff9800; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔐 One-Time Password</h1>
                    </div>
                    <div class='content'>
                        <p>Hello <strong>{userName}</strong>,</p>
                        <p>Your One-Time Password (OTP) is:</p>
                        <div class='otp-box'>
                            <div class='otp-code'>{otp}</div>
                        </div>
                        <div class='security-info'>
                            <p><strong>⏱️ Expires in:</strong> {expirationMinutes} minutes</p>
                            <p><strong>🔒 Security Reminder:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Never share this OTP with anyone</li>
                                <li>This is a one-time code and cannot be reused</li>
                                <li>Our team will never ask you for this code</li>
                            </ul>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
            }
        }
}
