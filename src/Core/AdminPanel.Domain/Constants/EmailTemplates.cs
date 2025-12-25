namespace AdminPanel.Domain.Constants;

public static class EmailTemplates
{
    public static string BaseTemplate(string content, string title = "لوحة التحكم")
    {
        return $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #007bff, #0056b3); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; background-color: #007bff; color: white !important; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .info {{ background-color: #e7f3ff; border: 1px solid #007bff; padding: 15px; border-radius: 5px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>{title}</h1></div>
        <div class='content'>{content}</div>
        <div class='footer'>
            <p>هذا البريد الإلكتروني تم إرساله تلقائياً، يرجى عدم الرد عليه.</p>
            <p>&copy; {DateTime.Now.Year} لوحة التحكم - جميع الحقوق محفوظة</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string PasswordReset(string userName, string resetLink, int expiryHours = 1)
    {
        var content = $@"
            <h2>مرحباً {userName}،</h2>
            <p>لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك.</p>
            <p>اضغط على الزر أدناه لإعادة تعيين كلمة المرور:</p>
            <p style='text-align: center;'><a href='{resetLink}' class='button'>إعادة تعيين كلمة المرور</a></p>
            <div class='warning'>
                <strong>⚠️ تنبيه:</strong>
                <ul><li>هذا الرابط صالح لمدة {expiryHours} ساعة فقط.</li><li>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد.</li></ul>
            </div>";
        return BaseTemplate(content, Messages.Titles.ResetPassword);
    }

    public static string Welcome(string userName, string loginUrl)
    {
        var content = $@"
            <h2>مرحباً {userName}! 🎉</h2>
            <p>نرحب بك في نظامنا، تم إنشاء حسابك بنجاح.</p>
            <div class='info'>
                <strong>ℹ️ معلومات الحساب:</strong>
                <ul><li>اسم المستخدم: <strong>{userName}</strong></li><li>يمكنك تسجيل الدخول الآن والبدء في استخدام النظام.</li></ul>
            </div>
            <p style='text-align: center;'><a href='{loginUrl}' class='button'>تسجيل الدخول</a></p>";
        return BaseTemplate(content, "مرحباً بك في النظام");
    }

    public static string EmailConfirmation(string userName, string confirmationLink)
    {
        var content = $@"
            <h2>مرحباً {userName}،</h2>
            <p>شكراً لتسجيلك في نظامنا. يرجى تأكيد بريدك الإلكتروني للمتابعة.</p>
            <p style='text-align: center;'><a href='{confirmationLink}' class='button' style='background-color: #28a745;'>تأكيد البريد الإلكتروني</a></p>
            <div class='warning'><strong>⚠️ ملاحظة:</strong> هذا الرابط صالح لمدة 24 ساعة فقط.</div>";
        return BaseTemplate(content, "تأكيد البريد الإلكتروني");
    }

    public static string PasswordChanged(string userName)
    {
        var content = $@"
            <h2>مرحباً {userName}،</h2>
            <p>تم تغيير كلمة المرور الخاصة بحسابك بنجاح.</p>
            <div class='info'><strong>✅ التفاصيل:</strong><ul><li>تم التغيير في: {DateTime.Now:yyyy-MM-dd HH:mm}</li></ul></div>
            <div class='warning'><strong>⚠️ تنبيه أمني:</strong><p>إذا لم تقم بهذا التغيير، يرجى التواصل مع فريق الدعم فوراً.</p></div>";
        return BaseTemplate(content, "تم تغيير كلمة المرور");
    }

    public static string Notification(string userName, string title, string message, string? actionUrl = null, string? actionText = null)
    {
        var actionButton = string.IsNullOrEmpty(actionUrl) ? "" : $"<p style='text-align: center;'><a href='{actionUrl}' class='button'>{actionText ?? "عرض التفاصيل"}</a></p>";
        var content = $"<h2>مرحباً {userName}،</h2><p>{message}</p>{actionButton}";
        return BaseTemplate(content, title);
    }
}
