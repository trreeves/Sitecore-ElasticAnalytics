namespace ElasticAnalytics.Model.Contact
{
    public enum AuthenticationLevel
    {
        /// <summary>
        /// Undefined authentication level.
        /// </summary>
        None = 0,

        /// <summary>
        /// Email has been validated.
        /// </summary>
        EmailValidated = 1,

        /// <summary>
        /// Password has been validated.
        /// </summary>
        PasswordValidated = 2,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        Reserved1 = 4,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        Reserved2 = 8,

        /// <summary>
        /// Custom validation.
        /// </summary>
        Custom1 = 16,

        /// <summary>
        /// Custom validation.
        /// </summary>
        Custom2 = 32
    }
}
