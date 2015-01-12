namespace ElasticAnalytics.Model.Locking
{
    public class ElasticLeaseOwner
    {
        protected readonly string _id;

        protected readonly LeaseOwnerType _type;

        public ElasticLeaseOwner(string id, LeaseOwnerType type)
        {
            this._id = id;
            this._type = type;
        }

        public virtual string Id
        {
            get
            {
                return this._id;
            }
        }

        public virtual LeaseOwnerType Type
        {
            get
            {
                return this._type;
            }
        }
    }
}
