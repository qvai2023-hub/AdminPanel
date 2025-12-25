namespace AdminPanel.Domain.Constants;

/// <summary>
/// رسائل النظام المركزية
/// </summary>
public static class Messages
{
    public static class Success
    {
        public const string LoginSuccess = "تم تسجيل الدخول بنجاح";
        public const string LogoutSuccess = "تم تسجيل الخروج بنجاح";
        public const string PasswordChanged = "تم تغيير كلمة المرور بنجاح";
        public const string PasswordReset = "تم إعادة تعيين كلمة المرور بنجاح";
        public const string EmailConfirmed = "تم تأكيد البريد الإلكتروني بنجاح";
        public const string PasswordResetLinkSent = "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة التعيين";
        public const string PasswordResetEmailSent = "تم إرسال رابط إعادة تعيين كلمة المرور";

        public const string Created = "تم الإنشاء بنجاح";
        public const string Updated = "تم التحديث بنجاح";
        public const string Deleted = "تم الحذف بنجاح";

        public const string UserCreated = "تم إنشاء المستخدم بنجاح";
        public const string UserUpdated = "تم تحديث المستخدم بنجاح";
        public const string UserDeleted = "تم حذف المستخدم بنجاح";
        public const string UserActivated = "تم تفعيل المستخدم بنجاح";
        public const string UserDeactivated = "تم تعطيل المستخدم بنجاح";
        public const string RolesAssigned = "تم تحديث الأدوار بنجاح";

        public const string RoleCreated = "تم إنشاء الدور بنجاح";
        public const string RoleUpdated = "تم تحديث الدور بنجاح";
        public const string RoleDeleted = "تم حذف الدور بنجاح";
        public const string PermissionsAssigned = "تم تحديث الصلاحيات بنجاح";

        public const string ActionCreated = "تم إنشاء الإجراء بنجاح";
        public const string ActionUpdated = "تم تحديث الإجراء بنجاح";
        public const string ActionDeleted = "تم حذف الإجراء بنجاح";
        public const string ActionActivated = "تم تفعيل الإجراء بنجاح";
        public const string ActionDeactivated = "تم تعطيل الإجراء بنجاح";

        public const string FileUploaded = "تم رفع الملف بنجاح";
        public const string FileDeleted = "تم حذف الملف بنجاح";
        public const string ExportSuccess = "تم التصدير بنجاح";
        public const string ImportSuccess = "تم الاستيراد بنجاح";
    }

    public static class Error
    {
        public const string GeneralError = "حدث خطأ غير متوقع";
        public const string NotFound = "العنصر غير موجود";
        public const string Unauthorized = "غير مصرح لك بالوصول";
        public const string Forbidden = "ليس لديك صلاحية لهذا الإجراء";
        public const string ValidationError = "خطأ في التحقق من البيانات";

        public const string InvalidCredentials = "اسم المستخدم أو كلمة المرور غير صحيحة";
        public const string AccountDisabled = "الحساب غير مفعل";
        public const string AccountLocked = "الحساب مقفل مؤقتاً. يرجى المحاولة لاحقاً";
        public const string InvalidToken = "التوكن غير صالح";
        public const string TokenExpired = "التوكن منتهي الصلاحية";
        public const string InvalidResetLink = "الرابط غير صالح أو منتهي الصلاحية";
        public const string PasswordMismatch = "كلمة المرور غير متطابقة";
        public const string CurrentPasswordWrong = "كلمة المرور الحالية غير صحيحة";

        public const string UserNotFound = "المستخدم غير موجود";
        public const string UsernameExists = "اسم المستخدم مستخدم مسبقاً";
        public const string EmailExists = "البريد الإلكتروني مستخدم مسبقاً";
        public const string PhoneExists = "رقم الجوال مستخدم مسبقاً";

        public const string RoleNotFound = "الدور غير موجود";
        public const string RoleNameExists = "اسم الدور مستخدم مسبقاً";
        public const string SystemRoleCannotBeModified = "لا يمكن تعديل أدوار النظام";
        public const string SystemRoleCannotBeDeleted = "لا يمكن حذف أدوار النظام";
        public const string RoleHasUsers = "لا يمكن حذف دور مرتبط بمستخدمين";

        public const string PermissionNotFound = "الصلاحية غير موجودة";

        public const string ActionNotFound = "الإجراء غير موجود";
        public const string ActionCodeExists = "كود الإجراء مستخدم مسبقاً";
        public const string ActionHasPageActions = "لا يمكن حذف إجراء مرتبط بصفحات";

        public const string FileNotFound = "الملف غير موجود";
        public const string FileTypeNotAllowed = "نوع الملف غير مسموح به";
        public const string FileTooLarge = "حجم الملف كبير جداً";
    }

    public static class Validation
    {
        public const string Required = "{0} مطلوب";
        public const string UsernameRequired = "اسم المستخدم مطلوب";
        public const string EmailRequired = "البريد الإلكتروني مطلوب";
        public const string PasswordRequired = "كلمة المرور مطلوبة";
        public const string FullNameRequired = "الاسم الكامل مطلوب";
        public const string RoleNameRequired = "اسم الدور مطلوب";

        public const string InvalidEmail = "البريد الإلكتروني غير صالح";
        public const string InvalidPhone = "رقم الجوال غير صالح";

        public const string UsernameMinLength = "اسم المستخدم يجب أن يكون 3 أحرف على الأقل";
        public const string UsernameMaxLength = "اسم المستخدم يجب ألا يتجاوز 50 حرف";
        public const string UsernameFormat = "اسم المستخدم يجب أن يحتوي على أحرف وأرقام فقط";

        public const string PasswordMinLength = "كلمة المرور يجب أن تكون 6 أحرف على الأقل";
        public const string PasswordMaxLength = "كلمة المرور يجب ألا تتجاوز 100 حرف";
        public const string PasswordUppercase = "كلمة المرور يجب أن تحتوي على حرف كبير على الأقل";
        public const string PasswordLowercase = "كلمة المرور يجب أن تحتوي على حرف صغير على الأقل";
        public const string PasswordDigit = "كلمة المرور يجب أن تحتوي على رقم على الأقل";
        public const string PasswordConfirmMismatch = "كلمة المرور غير متطابقة";

        public const string FullNameMinLength = "الاسم الكامل يجب أن يكون 2 أحرف على الأقل";
        public const string FullNameMaxLength = "الاسم الكامل يجب ألا يتجاوز 100 حرف";
    }

    public static class Confirm
    {
        public const string DeleteUser = "هل أنت متأكد من حذف هذا المستخدم؟";
        public const string DeleteRole = "هل أنت متأكد من حذف هذا الدور؟";
        public const string DeactivateUser = "هل أنت متأكد من تعطيل هذا المستخدم؟";
        public const string Logout = "هل أنت متأكد من تسجيل الخروج؟";
        public const string DiscardChanges = "هل أنت متأكد؟ سيتم فقدان التغييرات غير المحفوظة";
    }

    public static class Titles
    {
        public const string Dashboard = "لوحة التحكم";
        public const string Users = "المستخدمين";
        public const string CreateUser = "إضافة مستخدم";
        public const string EditUser = "تعديل مستخدم";
        public const string Roles = "الأدوار";
        public const string CreateRole = "إضافة دور";
        public const string EditRole = "تعديل دور";
        public const string Permissions = "الصلاحيات";
        public const string AuditLogs = "سجل التدقيق";
        public const string Settings = "الإعدادات";
        public const string Profile = "الملف الشخصي";
        public const string Login = "تسجيل الدخول";
        public const string ForgotPassword = "نسيت كلمة المرور";
        public const string ResetPassword = "إعادة تعيين كلمة المرور";
    }

    public static class Fields
    {
        public const string Username = "اسم المستخدم";
        public const string Email = "البريد الإلكتروني";
        public const string Password = "كلمة المرور";
        public const string ConfirmPassword = "تأكيد كلمة المرور";
        public const string FullName = "الاسم الكامل";
        public const string PhoneNumber = "رقم الجوال";
        public const string IsActive = "الحالة";
        public const string Roles = "الأدوار";
        public const string Permissions = "الصلاحيات";
        public const string CreatedAt = "تاريخ الإنشاء";
        public const string Actions = "الإجراءات";
    }

    public static class Buttons
    {
        public const string Save = "حفظ";
        public const string Cancel = "إلغاء";
        public const string Delete = "حذف";
        public const string Edit = "تعديل";
        public const string Create = "إضافة";
        public const string Search = "بحث";
        public const string Export = "تصدير";
        public const string Import = "استيراد";
        public const string Back = "رجوع";
        public const string Login = "دخول";
        public const string Logout = "خروج";
        public const string Yes = "نعم";
        public const string No = "لا";
        public const string Confirm = "تأكيد";
        public const string Close = "إغلاق";
    }
}
