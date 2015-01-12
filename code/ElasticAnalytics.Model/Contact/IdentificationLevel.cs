namespace ElasticAnalytics.Model.Contact
{
    public enum IdentificationLevel
    {
        /// <summary>
        ///   No attempt has been made yet to identify the contact.
        /// </summary>
        None = 0,

        /// <summary>
        ///   Contact has failed to be identified.
        /// </summary>
        Anonymous = 1,

        /// <summary>
        ///   Contact has been identified.
        /// </summary>
        Known = 2
    }
}
