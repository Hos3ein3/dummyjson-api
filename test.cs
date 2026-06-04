using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

class Program {
    static void Main() {
        var services = new ServiceCollection();
        services.AddHealthChecks().AddMongoDb("mongodb://localhost");
    }
}
