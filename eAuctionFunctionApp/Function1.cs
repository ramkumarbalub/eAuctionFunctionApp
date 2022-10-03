using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;

namespace eAuctionFunctionApp
{
    public static class Function1
    {
        public static readonly string serverURI = "mongodb://eauction-cosmos-mongo:YEzhfAB9jZKe0TPmWkcXtm31bsnPA97bH4nWjJmi9fOcuNLQnbyEt4XuFLnJeQWhMeOCKSbjl7zD5JEeVLsV9Q==@eauction-cosmos-mongo.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@eauction-cosmos-mongo@";
        public static readonly string database = "eAuction";

        [FunctionName("InsertBuyerForAuction")]
        public static void InsertBuyerForAuction([ServiceBusTrigger("buyerplacingbidq", Connection = "ServiceBusConnection")] string biddingProductInfo,
            ILogger log)
        {
            log.LogInformation($"Begin - Deserializing the message from buyerplacingbidq. The product details are - {biddingProductInfo}");
            BuyerModel biddingProductModelInfo = JsonConvert.DeserializeObject<BuyerModel>(biddingProductInfo);
            log.LogInformation($"End - Deserializing the message from buyerplacingbidq. The productId is - {biddingProductModelInfo.productid}");

            IMongoCollection<BuyerModel> _biddingProductCollection;

           
            string collectionName = "buyers";
            MongoClient mongoClient = new MongoClient(serverURI);
            var mongoDatabase = mongoClient.GetDatabase(database);
            _biddingProductCollection = mongoDatabase.GetCollection<BuyerModel>(collectionName); //productv2

            _biddingProductCollection.InsertOneAsync(biddingProductModelInfo);
        }
        [FunctionName("InsertProductsWithBidPrice")]
        public static void InsertProductsWithBidPrice([ServiceBusTrigger("addproductq", Connection = "ServiceBusConnection")] string productForAuction,
            ILogger log)
        {
            log.LogInformation($"Begin - Deserializing the message from addproductq. The product details are - {productForAuction}");
            ProductModel ProductModelInfo = JsonConvert.DeserializeObject<ProductModel>(productForAuction);
            log.LogInformation($"End - Deserializing the message from addproductq. The productId is - {ProductModelInfo.productId}");

            IMongoCollection<ProductModel> _productCollection;

             //   string serverURI = "mongodb://eauction-cosmos-mongo:YEzhfAB9jZKe0TPmWkcXtm31bsnPA97bH4nWjJmi9fOcuNLQnbyEt4XuFLnJeQWhMeOCKSbjl7zD5JEeVLsV9Q==@eauction-cosmos-mongo.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@eauction-cosmos-mongo@";
             //  string database = "eAuction";
               string collectionName = "products";
                MongoClient mongoClient = new MongoClient(serverURI);
           var mongoDatabase = mongoClient.GetDatabase(database);
            _productCollection = mongoDatabase.GetCollection<ProductModel>(collectionName); //productv2

            _productCollection.InsertOneAsync(ProductModelInfo);


        }
        [FunctionName("DeleteBid")]
        public static void DeleteBid([ServiceBusTrigger("deletebidq", Connection = "ServiceBusConnection")] string deleteBidInfo,
            ILogger log)
        {
            log.LogInformation($"Begin - Deserializing the message from deletebidq. The product details are - {deleteBidInfo}");
            UpdateBidModel NewBidInfoModel = JsonConvert.DeserializeObject<UpdateBidModel>(deleteBidInfo);
            log.LogInformation($"End - Deserializing the message from deletebidq. The productId is - {NewBidInfoModel.productId}");


            //Connect the MongoDB
            // string serverURI = "mongodb://localhost:27017/";
            // string database = "eAuction";
            string collectionName = "buyers";
            MongoClient client = new MongoClient(serverURI);

            //Connect the Particular database
            MongoServer myMongoServer = client.GetServer();

            MongoDefaults.MaxConnectionIdleTime = TimeSpan.FromMinutes(3);

            if (myMongoServer.State == MongoServerState.Disconnected)
                myMongoServer.Connect();

            MongoDatabase myMongoDatabase = myMongoServer.GetDatabase(database);

            //Access the particular collection
            MongoCollection<BsonDocument> myMongoCollection = myMongoDatabase.GetCollection<BsonDocument>(collectionName);


            //Update the product based on the ProductId
            myMongoCollection.Remove(Query.And(Query.EQ("productid", NewBidInfoModel.productId), Query.EQ("email", NewBidInfoModel.emailId)));

        }

        [FunctionName("UpdateBid")]
        public static void UpdateBid([ServiceBusTrigger("updatebidq", Connection = "ServiceBusConnection")] string newBidInfo,
            ILogger log)
        {
            log.LogInformation($"Begin - Deserializing the message from updatebidq. The product details are - {newBidInfo}");
            UpdateBidModel NewBidInfoModel = JsonConvert.DeserializeObject<UpdateBidModel>(newBidInfo);
            log.LogInformation($"End - Deserializing the message from updatebidq. The productId is - {NewBidInfoModel.productId}");

            
                //Connect the MongoDB
               // string serverURI = "mongodb://localhost:27017/";
               // string database = "eAuction";
                string collectionName = "buyers";
                MongoClient client = new MongoClient(serverURI);

                //Connect the Particular database
                MongoServer myMongoServer = client.GetServer();

                MongoDefaults.MaxConnectionIdleTime = TimeSpan.FromMinutes(3);

                if (myMongoServer.State == MongoServerState.Disconnected)
                    myMongoServer.Connect();

                MongoDatabase myMongoDatabase = myMongoServer.GetDatabase(database);

                //Access the particular collection
                MongoCollection<BsonDocument> myMongoCollection = myMongoDatabase.GetCollection<BsonDocument>(collectionName);


            //Update the product based on the ProductId
            myMongoCollection.Update(Query.And(Query.EQ("productid", NewBidInfoModel.productId), Query.EQ("email", NewBidInfoModel.emailId)), Update.Set("bidamount", NewBidInfoModel.bidPrice));

        }

        [FunctionName("GetProducts")]
        public static void GetProducts([ServiceBusTrigger("productsq", Connection = "ServiceBusConnection")] string myQueueItem,
            ILogger log)        
        {
            try
            {
                log.LogInformation($"Begin - C# ServiceBus queue trigger function processed message: {myQueueItem}");
                ProductModel ProductModelInfo = JsonConvert.DeserializeObject<ProductModel>(myQueueItem);
                log.LogInformation($"Begin - C# ServiceBus queue trigger function processed - {ProductModelInfo.productId}");

                log.LogInformation($"Begin - Cosmos DB");
                //documentsDelete.
                //Connect the MongoDB
                string serverURI = "mongodb://eauction-cosmos-mongo:YEzhfAB9jZKe0TPmWkcXtm31bsnPA97bH4nWjJmi9fOcuNLQnbyEt4XuFLnJeQWhMeOCKSbjl7zD5JEeVLsV9Q==@eauction-cosmos-mongo.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@eauction-cosmos-mongo@";
                string database = "eAuction";
                string collectionName = "products";
                MongoClient client = new MongoClient(serverURI);

               

                //Connect the Particular database
                MongoServer myMongoServer = client.GetServer();

                MongoDefaults.MaxConnectionIdleTime = TimeSpan.FromMinutes(3);

                if (myMongoServer.State == MongoServerState.Disconnected)
                    myMongoServer.Connect();

                MongoDatabase myMongoDatabase = myMongoServer.GetDatabase(database);

                //Access the particular collection
                MongoCollection<BsonDocument> myMongoCollection = myMongoDatabase.GetCollection<BsonDocument>(collectionName);

                
                //List all the bids against the product             
                var BidsList = myMongoCollection.Find(Query.EQ("productId", ProductModelInfo.productId))
                    .SetFields(Fields.Include("productId", "productname", "shortdescription",
                    "detaileddescription").Exclude("_id")).ToList();
                log.LogInformation($"End - Cosmos DB - {BidsList.ToJson()} ");
            }
            catch(Exception ex)
            {
                log.LogError($"Exception is {ex.Message}, {ex.InnerException}");
            }
        }
    }
}
