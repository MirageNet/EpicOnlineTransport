#region Statements

using Epic.OnlineServices;

#endregion

namespace EpicChill.EpicCore
{
    public class EpicUser
    {
        #region Class Specific

        /// <summary>
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="productId"></param>
        public EpicUser(EpicAccountId accountId, ProductUserId productId)
        {
            AccountId = accountId;
            ProductUserId = productId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public EpicAccountId AccountId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ProductUserId ProductUserId { get; private set; }

        /// <summary>
        /// </summary>
        public string Name { get; internal set; }

        #endregion
    }
}
