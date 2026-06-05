using MongoDB.Driver;
using System;

class Program {
    static void Main() {
        var connectionString = "mongodb://homelabadmin:123456789012@100.76.178.87:27017/DummyJsonDb?authSource=admin";
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase("DummyJsonDb");
        db.DropCollection("comments");
        Console.WriteLine("Comments collection dropped.");
    }
}
