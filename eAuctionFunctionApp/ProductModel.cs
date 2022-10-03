using System;
using System.Collections.Generic;
using System.Text;

namespace eAuctionFunctionApp
{
    public class ProductModel
    {
        public string productId { get; set; }
        public string productname { get; set; }
        //shortdescription
        public string shortdescription { get; set; }
        //detaileddescription
        public string detaileddescription { get; set; }
        //category
        public string category { get; set; }
        //startingprice
        public string startingprice { get; set; }
        //bidenddate
        public string bidenddate { get; set; }
    }
}
