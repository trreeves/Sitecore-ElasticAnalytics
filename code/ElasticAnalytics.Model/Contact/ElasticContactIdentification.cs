namespace ElasticAnalytics.Model.Contact
{
    using System;

    public class ElasticContactIdentification
    {
        protected AuthenticationLevel authLevel;

        protected IdentificationLevel identityLevel;

        protected string identity;

        public ElasticContactIdentification(
            string identity, 
            AuthenticationLevel authLevel, 
            IdentificationLevel idLevel)
        {
            if (string.IsNullOrWhiteSpace(identity) && idLevel != IdentificationLevel.None)
            {
                throw new ArgumentOutOfRangeException("identity", "A contact identity value must be supplied to match the identification level of " + idLevel);    
            }

            this.authLevel = authLevel;
            this.SetIdentity(identity, idLevel);
        }

        public void SetIdentity(string newIdentity, IdentificationLevel idLevel)
        {
            if (string.IsNullOrWhiteSpace(newIdentity) && idLevel != IdentificationLevel.None)
            {
                throw new ArgumentOutOfRangeException("newIdentity", "A contact identity value must be supplied to match the identification level of " + idLevel);
            }

            this.identity = newIdentity;
            this.identityLevel = idLevel;
        }

        public void RemoveIdentity()
        {
            this.identity = null;
            this.identityLevel = IdentificationLevel.None;
        }

        public virtual string Identity
        {
            get
            {
                return this.identity;
            }
        }

        public virtual AuthenticationLevel AuthLevel
        {
            get
            {
                return this.authLevel;
            }
        }

        public virtual IdentificationLevel IdentityLevel
        {
            get
            {
                return this.identityLevel;
            }
        }
    }
}
