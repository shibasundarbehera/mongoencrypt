using encrypt_test;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;

const string LocalMasterKey = "Mng0NCt4ZHVUYUJCa1kxNkVyNUR1QURhZ2h2UzR2d2RrZzh0cFBwM3R6NmdWMDFBMUN3YkQ5aXRRMkhGRGdQV09wOGVNYUMxT2k3NjZKelhaQmRCZGJkTXVyZG9uSjFk";

var localMasterKey = Convert.FromBase64String(LocalMasterKey);

var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
var localKey = new Dictionary<string, object>
            {
                { "key", localMasterKey }
            };

kmsProviders.Add("local", localKey);

var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");
var keyVaultClient = new MongoClient("mongodb://192.168.30.50:27017");

// Create two data keys.
var clientEncryptionOptions = new ClientEncryptionOptions(keyVaultClient, keyVaultNamespace, kmsProviders);
using var clientEncryption = new ClientEncryption(clientEncryptionOptions);

var dataKeyOptions = new DataKeyOptions();

// Check if the data key already exists in the key vault
var filter = Builders<BsonDocument>.Filter.Eq("masterKey.provider", "local");

var dataKey = new Guid();
var dataKeyDocument = keyVaultClient.GetDatabase(keyVaultNamespace.DatabaseNamespace.DatabaseName)
    .GetCollection<BsonDocument>(keyVaultNamespace.CollectionName)
    .Find(filter)
    .FirstOrDefault();

if (dataKeyDocument != null)
{
    // Data key already exists, use it
    var dataKeyGuid = dataKeyDocument["_id"].AsGuid;
    dataKey =dataKeyGuid;
   // var dataKeyId = new BsonBinaryData(dataKeyGuid, GuidRepresentation.Standard);
     
}
else
{
    // Data key doesn't exist, create a new one
    dataKey = clientEncryption.CreateDataKey("local", dataKeyOptions, CancellationToken.None);
}

//var dataKey = clientEncryption.CreateDataKey("local", dataKeyOptions, CancellationToken.None);
Console.WriteLine(dataKey.ToJson());

var encryptedFieldsMap = new Dictionary<string, BsonDocument>();

encryptedFieldsMap.Add("ProductRnD.SecurityBooks", new BsonDocument()
                    {
                        {
                            "fields",
                            new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "path", "Price" },
                                    { "keyId", new BsonBinaryData(dataKey, GuidRepresentation.Standard) }
                                },
                                new BsonDocument
                                {
                                   { "path", "Publisher" },
                                   { "keyId", new BsonBinaryData(dataKey, GuidRepresentation.Standard) } 
                                }
                            }
                        }
                    });

 scemaMapValue.Add("ProductRnD.SecurityBooks",new BsonDocument(){
            { "bsonType", "object" },
            {
                "encryptMetadata",
                new BsonDocument("keyId", new BsonArray(new[] { new BsonBinaryData(dataKey, GuidRepresentation.Standard) }))
            },
            {
                "properties",
                new BsonDocument
                {
                    {
                        "Publisher", new BsonDocument
                        {
                            {
                                "encrypt", new BsonDocument
                                {
                                    { "bsonType", "string" },
                                    { "algorithm", "AEAD_AES_256_CBC_HMAC_SHA_512-Deterministic" }
                                }
                            }
                        }
                    }
                }
           },
});                

//var ExtraOptions = new Dictionary<string, object>
            // {
            //     {"mongocryptdURI", "mongodb://localhost:27020" }
            // };
//var autoEncryptionOptions = new AutoEncryptionOptions(keyVaultNamespace, kmsProviders, encryptedFieldsMap: encryptedFieldsMap, extraOptions: null);
 
// var mongoClientSettings = new MongoClientSettings
// {
//     AutoEncryptionOptions = autoEncryptionOptions,
//     Server = new MongoServerAddress("192.168.30.50", 27017),
// };

//var client = new MongoClient(mongoClientSettings);


var clientSettings = MongoClientSettings.FromConnectionString("mongodb://192.168.30.50:27017/ProductRnD");
var autoEncryptionOptions = new AutoEncryptionOptions(
    keyVaultNamespace: keyVaultNamespace,
    kmsProviders: kmsProviders,
    encryptedFieldsMap: encryptedFieldsMap,
    extraOptions: null);
clientSettings.AutoEncryptionOptions = autoEncryptionOptions;
var client = new MongoClient(clientSettings);

var database = client.GetDatabase("ProductRnD");

//database.DropCollection("SecurityBooks");

var collection = database.GetCollection<Book>("SecurityBooks");

//collection.InsertOne(new Book("My new book50!", "Tweedle Dum", 56.90f,"Pearson"));
//collection.InsertOne(new Book("My new book51!", "Tweedle Dum", 47.90f,"Ajanta"));

//var filterDefinition = Builders<Book>.Filter.Gt("Price", 30);
//var result = collection.Find(filterDefinition).ToList();
var result = collection.Find(FilterDefinition<Book>.Empty).ToList();
Console.WriteLine(result.ToJson());
// BsonDocument filternew = new BsonDocument();
//            // filternew.Add("Title", "My new book42!");
//            filternew.Add("Publisher", new BsonRegularExpression(".*Tweedle.*", "i"));
//             result = collection.Find(filternew).ToList();
            

Console.WriteLine(result.ToJson());

//Console.ReadKey();
