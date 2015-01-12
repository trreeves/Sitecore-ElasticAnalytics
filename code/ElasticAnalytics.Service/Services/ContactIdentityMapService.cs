using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Service.Services
{
    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Utils.DateTime;

    public class ContactIdentityMapService : IContactIdentityMapService
    {
        private readonly IRepository<ElasticContactIdentityMap> identityMapRepo;

        public ContactIdentityMapService(
            IRepository<ElasticContactIdentityMap> identityMapRepo)
        {
            if (identityMapRepo == null)
            {
                throw new ArgumentNullException("identityMapRepo");
            }

            this.identityMapRepo = identityMapRepo;
        }

        public ElasticContactIdentityMap Get(string identity, ISystemContext ctx)
        {
            return this.identityMapRepo.Get(identity, ctx);
        }

        public void Delete(string identity, ISystemContext ctx)
        {
            this.identityMapRepo.Delete(identity, ctx);
        }

        public bool MapIdentity(
            Guid targetContactId,
            ElasticContactIdentification targetIdentification,
            ElasticContactIdentification currentIdentification,
            ISystemContext ctx)
        {
            // contact has identity
            if (targetIdentification.IdentityLevel != IdentificationLevel.None)
            {
                // get the current stored identity map for the target Identity
                var currentTargetMap = this.Get(targetIdentification.Identity, ctx);

                if (currentTargetMap == null)
                {
                    currentTargetMap = new ElasticContactIdentityMap(targetIdentification.Identity, targetContactId);

                    if (!this.identityMapRepo.Create(currentTargetMap, ctx))
                    {
                        // This map was subsequently created by someone else after our initial check
                        return this.MapIdentity(targetContactId,targetIdentification, currentIdentification, ctx);
                    }
                }
                else if (currentTargetMap.ContactId != targetContactId)
                {
                    // map already exists, but for another contact, which is an error
                    // (we can safely assume that this contact exists because we are in control of 
                    // deleting identity maps when a contact gets deleted).
                    return false;                    
                }

                // else map already exists, in the correct state.
            }

            // clean up the previous identity - we're replacing it with either a new identity or potentially nothing.
            if (currentIdentification != null && 
                currentIdentification.Identity != targetIdentification.Identity) // (target identity could be empty here)
            {
                this.Delete(currentIdentification.Identity, ctx);
            }

            return true;
        }
    }
}
