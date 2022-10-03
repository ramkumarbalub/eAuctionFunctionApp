using System;
using System.Collections.Generic;
using System.Text;

namespace eAuctionFunctionApp
{
    public class UpdateBidModel
    {
        public string productId { get; set; }
        public string emailId { get; set; }
        public string bidPrice { get; set; }
    }
}
