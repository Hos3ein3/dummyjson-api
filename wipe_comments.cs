using MongoDB.Driver;
using System;

class Program {
    static void Main() {
        var client = new MongoClient("mongodb://homelabadmin:123456789012@100.76.178.87:27017/DummyJsonDb.dev?authSource=admin");
        var db = client.GetDatabase("DummyJsonDb.dev");
        db.DropCollection("comments");
        Console.WriteLine("Comments collection dropped.");
    }
}
