using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TG_Fitz.ReportService;
using TG_Fitz.ReportService.Models;

Console.WriteLine("📄 ReportService started. Listening for report_tasks...");

var factory = new ConnectionFactory {
    HostName = "localhost",
    DispatchConsumersAsync = true
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "report_tasks",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.Received += async (model, ea) => {
    try {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        var reportMessage = JsonSerializer.Deserialize<ReportMessage>(json);
        if (reportMessage == null) {
            Console.WriteLine("⚠️ Invalid message received.");
            return;
        }

        var path = ReportGenerator.GeneratePdf(reportMessage);
        Console.WriteLine($"✅ PDF generated: {path}");

        channel.BasicAck(ea.DeliveryTag, multiple: false);
    } catch (Exception ex) {
        Console.WriteLine("❌ Error occurred:");
        Console.WriteLine(ex);
    }
};

channel.BasicConsume(queue: "report_tasks",
                     autoAck: false,
                     consumer: consumer);

await Task.Delay(-1); // keep app running
