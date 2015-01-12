namespace ElasticAnalytics.Model.Contact
{
    using System;
    using System.Collections.Generic;

    using ElasticAnalytics.Model.Locking;

    using Newtonsoft.Json.Linq;

    public class ElasticContact
    {
        protected readonly Guid id;

        protected Guid? successor;

        protected readonly bool obsolete = false;

        protected ElasticLease lease;

        protected readonly ElasticContactIdentification identification;

        protected JObject metadata;

        protected JObject facets;

        public ElasticContact(
            Guid id, 
            ElasticContactIdentification identification)
        {
            this.id = id;
            this.identification = identification;
            this.metadata = new JObject();
            this.facets = new JObject();
        }

        /// <summary>Creates an obsolete contact.</summary>
        public ElasticContact(Guid id, Guid? successor)
        {
            this.id = id;
            this.successor = successor;
            this.obsolete = true;
            this.metadata = new JObject();
            this.facets = new JObject();
        }

        public virtual Guid Id
        {
            get
            {
                return this.id;
            }
        }

        public virtual Guid? Successor
        {
            get
            {
                return this.successor;
            }
            set
            {
                this.successor = value;
            }
        }

        public virtual bool Obsolete
        {
            get
            {
                return this.obsolete;
            }
        }

        public virtual ElasticLease Lease
        {
            get
            {
                return this.lease;
            }
            set
            {
                this.lease = value;
            }
        }

        public virtual ElasticContactIdentification Identification
        {
            get
            {
                return this.identification;
            }
        }

        public JObject Metadata
        {
            get
            {
                return this.metadata;
            }
            set
            {
                this.metadata = value ?? new JObject();
            }
        }

        public JObject Facets
        {
            get
            {
                return this.facets;
            }
            set
            {
                this.facets = value ?? new JObject();
            }
        }
    }
}
