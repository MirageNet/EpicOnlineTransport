#region Statements

using Epic.OnlineServices;

#endregion

namespace Epic.Core
{
    public class EpicUser
    {
        #region Class Specific

        /// <summary>
        /// </summary>
        /// <param name="epicAccountId"></param>
        /// <param name="productId"></param>
        public EpicUser(EpicAccountId epicAccountId, ProductUserId productId)
        {
            ProductUserId = productId;
            EpicAccountId = epicAccountId;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// 
        /// </summary>
        public ProductUserId ProductUserId { get; private set; }

        public EpicAccountId EpicAccountId { get; private set; }

        /// <summary>
        /// </summary>
        public string Name { get; internal set; }

        #endregion
    }
}
