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
        /// <param name="productId"></param>
        public EpicUser(ProductUserId productId)
        {
            ProductUserId = productId;
        }

        #endregion

        #region Properties
        
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
