using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
    public static class Templates
    {
        private const string CompanyName = "MR.X";
        private const string CompanyYear = "2026";

        public static string WelcomeEmailTemplate(string userName, string activationLink)
        {
            return $@"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; direction: rtl; text-align: right; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #007bff; padding-bottom: 20px; }}
                    .header h1 {{ color: #007bff; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; text-align: center; }}
                    .button-container {{ text-align: center; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>أهلاً بيك!</h1>
                    </div>
                    <div class='content'>
                        <p>أهلاً يا <strong>{userName}</strong>،</p>
                        <p>شكراً إنك سجلت معانا. إحنا مبسوطين جداً بوجودك!</p>
                        <p>يا ريت تأكد الإيميل بتاعك من خلال الضغط على الزرار اللي تحت:</p>
                        <div class='button-container'>
                            <a href='{activationLink}' class='button'>تأكيد الإيميل</a>
                        </div>
                        <p>لو معملتش الحساب ده، تقدر تتجاهل الإيميل ده بأمان.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. جميع الحقوق محفوظة.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string PasswordResetTemplate(string userName, string resetLink)
        {
            return $@"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; direction: rtl; text-align: right; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #dc3545; padding-bottom: 20px; }}
                    .header h1 {{ color: #dc3545; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .button {{ display: inline-block; background-color: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
                    .button-container {{ text-align: center; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                    .warning {{ background-color: #fff3cd; border-right: 4px solid #ffc107; padding: 10px; margin: 20px 0; text-align: right; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>طلب تغيير الباسورد</h1>
                    </div>
                    <div class='content'>
                        <p>أهلاً يا <strong>{userName}</strong>،</p>
                        <p>وصلنا طلب لتغيير الباسورد بتاعك. دوس على الزرار اللي تحت عشان تكمل:</p>
                        <div class='button-container'>
                            <a href='{resetLink}' class='button'>تغيير الباسورد</a>
                        </div>
                        <div class='warning'>
                            <p><strong>ملاحظة:</strong> اللينك ده هينتهي بعد ساعة واحدة. لو مطلبش التغيير ده، يرجى تجاهل الإيميل.</p>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. جميع الحقوق محفوظة.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string ConfirmationEmailTemplate(string userName, string message)
        {
            return $@"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; direction: rtl; text-align: right; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #28a745; padding-bottom: 20px; }}
                    .header h1 {{ color: #28a745; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .success-box {{ background-color: #d4edda; border-right: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>✓ تأكيد</h1>
                    </div>
                    <div class='content'>
                        <p>أهلاً يا <strong>{userName}</strong>،</p>
                        <div class='success-box'>
                            <p>{message}</p>
                        </div>
                        <p>شكراً إنك بتستخدم خدماتنا!</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. جميع الحقوق محفوظة.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string NotificationEmailTemplate(string userName, string title, string content)
        {
            return $@"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; direction: rtl; text-align: right; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #17a2b8; padding-bottom: 20px; }}
                    .header h1 {{ color: #17a2b8; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .title {{ font-size: 18px; font-weight: bold; color: #17a2b8; margin: 20px 0 10px 0; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>إشعار جديد</h1>
                    </div>
                    <div class='content'>
                        <p>أهلاً يا <strong>{userName}</strong>،</p>
                        <div class='title'>{title}</div>
                        <p>{content}</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. جميع الحقوق محفوظة.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string OtpEmailTemplate(string userName, string otp)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #ff9800; padding-bottom: 20px; }}
                    .header h1 {{ color: #ff9800; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .otp-box {{ background-color: #fff3e0; border: 2px solid #ff9800; padding: 20px; border-radius: 8px; margin: 20px 0; text-align: center; }}
                    .otp-code {{ font-size: 32px; font-weight: bold; color: #ff9800; letter-spacing: 4px; font-family: 'Courier New', monospace; display: inline-block; }}
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
                            <p><strong>🔒 Security Reminder:</strong></p>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Never share this OTP with anyone</li>
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

        public static string BookingInfoTemplate(string barberName, string customerName, string customerPhone, DateOnly bookingDate, TimeOnly startTime)
        {
            return $@"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; direction: rtl; text-align: right; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #007bff; padding-bottom: 20px; }}
                    .header h1 {{ color: #007bff; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .booking-details {{ background-color: #f8f9fa; border-right: 4px solid #007bff; padding: 15px; margin: 20px 0; border-radius: 4px; }}
                    .detail-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #e9ecef; flex-direction: row-reverse; }}
                    .detail-row:last-child {{ border-bottom: none; }}
                    .detail-label {{ font-weight: bold; color: #495057; }}
                    .detail-value {{ color: #212529; direction: ltr; }}
                    .whatsapp-button {{ display: inline-block; background-color: #25d366; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; text-align: center; }}
                    .whatsapp-button:hover {{ background-color: #1ec857; }}
                    .button-container {{ text-align: center; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>📅 إشعار بحجز جديد</h1>
                    </div>
                    <div class='content'>
                        <p>أهلاً يا <strong>{barberName}</strong>،</p>
                        <p>عندك حجز جديد! دي التفاصيل:</p>
                        <div class='booking-details'>
                            <div class='detail-row'>
                                <span class='detail-value'>{customerName}</span>
                                <span class='detail-label'>:اسم العميل</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-value'>{customerPhone}</span>
                                <span class='detail-label'>:رقم الموبايل</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-value'>{bookingDate:yyyy-MM-dd}</span>
                                <span class='detail-label'>:تاريخ الحجز</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-value'>{startTime:hh:mm tt}</span>
                                <span class='detail-label'>:ميعاد البدء</span>
                            </div>
                        </div>
                        <p>يا ريت تتأكد إنك متاح في الميعاد ده. لو في أي مشكلة، يرجى التواصل مع العميل في أسرع وقت.</p>
                        <div class='button-container'>
                            <a href='https://wa.me/{FormatPhoneForWhatsApp(customerPhone)}' class='whatsapp-button'>💬 تواصل على واتساب</a>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. جميع الحقوق محفوظة.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string BookingCancellationTemplate(string barberName, string customerName, string customerPhone, DateOnly bookingDate, TimeOnly startTime)
        {
            return $@"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; direction: rtl; text-align: right; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; border-bottom: 2px solid #dc3545; padding-bottom: 20px; }}
                    .header h1 {{ color: #dc3545; margin: 0; }}
                    .content {{ padding: 20px 0; }}
                    .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                    .booking-details {{ background-color: #f8f9fa; border-right: 4px solid #dc3545; padding: 15px; margin: 20px 0; border-radius: 4px; }}
                    .detail-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #e9ecef; flex-direction: row-reverse; }}
                    .detail-row:last-child {{ border-bottom: none; }}
                    .detail-label {{ font-weight: bold; color: #495057; }}
                    .detail-value {{ color: #212529; direction: ltr; }}
                    .cancellation-notice {{ background-color: #f8d7da; border-right: 4px solid #dc3545; padding: 15px; margin: 20px 0; border-radius: 4px; }}
                    .whatsapp-button {{ display: inline-block; background-color: #25d366; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; text-align: center; }}
                    .whatsapp-button:hover {{ background-color: #1ec857; }}
                    .button-container {{ text-align: center; }}
                    .footer {{ text-align: center; border-top: 1px solid #ddd; padding-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>❌ إشعار بإلغاء حجز</h1>
                    </div>
                    <div class='content'>
                        <p>أهلاً يا <strong>{barberName}</strong>،</p>
                        <p>في حجز اتلغى. دي التفاصيل:</p>
                        <div class='booking-details'>
                            <div class='detail-row'>
                                <span class='detail-value'>{customerName}</span>
                                <span class='detail-label'>:اسم العميل</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-value'>{customerPhone}</span>
                                <span class='detail-label'>:رقم الموبايل</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-value'>{bookingDate:yyyy-MM-dd}</span>
                                <span class='detail-label'>:تاريخ الحجز</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-value'>{startTime:hh:mm tt}</span>
                                <span class='detail-label'>:ميعاد البدء</span>
                            </div>
                        </div>
                        <div class='cancellation-notice'>
                            <p><strong>الميعاد ده دلوقتي متاح</strong> ويقدر أي عميل تاني يحجزه.</p>
                        </div>
                        <div class='button-container'>
                            <a href='https://wa.me/{FormatPhoneForWhatsApp(customerPhone)}' class='whatsapp-button'>💬 تواصل على واتساب</a>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>&copy; {CompanyYear} {CompanyName}. جميع الحقوق محفوظة.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private static string FormatPhoneForWhatsApp(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            string formatted = phone.Replace("+", "").Replace(" ", "").Replace("-", "");
            
            // Handle Egyptian numbers that start with '01' and are 11 digits long
            if (formatted.StartsWith("01") && formatted.Length == 11)
            {
                formatted = "2" + formatted;
            }
            
            return formatted;
        }
    }
}
