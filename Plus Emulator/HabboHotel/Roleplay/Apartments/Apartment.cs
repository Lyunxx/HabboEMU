using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Roleplay.Apartments
{

    public class Apartment
    {

        #region Variables
        /// <summary>
        /// The roomid of the apartment
        /// </summary>
        public uint RoomId;

        /// <summary>
        /// The owners uid
        /// </summary>
        public uint OwnerId;

        /// <summary>
        /// Is the apartment VIP
        /// </summary>
        public bool VIP;

        /// <summary>
        /// Is the apartment for sale
        /// </summary>
        public bool ForSale;
        #endregion

        #region Constructor
        /// <summary>
        /// Initiializes variables
        /// </summary>
        /// <param name="RoomId">The roomid</param>
        /// <param name="OwnerId">The ownerid</param>
        /// <param name="VIP">Is the apartment vip</param>
        /// <param name="ForSale">Is the apartment for sale</param>
        public Apartment(uint RoomId, uint OwnerId, bool VIP, bool ForSale)
        {
            this.RoomId = RoomId;
            this.OwnerId = OwnerId;
            this.VIP = VIP;
            this.ForSale = ForSale;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the apartment is for sale
        /// </summary>
        /// <param name="RoomId">The room id of the apartment to check</param>
        /// <returns>True if yes, otherwise false</returns>
        public bool isForSale(uint RoomId)
        {
            List<uint> ForSaleList = new List<uint>();

            if (ForSale)
            {
                ForSaleList.Add(RoomId);
            }

            return ForSaleList.Contains(RoomId);
        }
        #endregion
    }
}
