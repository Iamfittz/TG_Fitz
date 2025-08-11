using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TG_Fitz.ReportService.Models;

namespace TG_Fitz.ReportService {
    public class TestProducer {
        public static void Main(string[] args) {
            Console.WriteLine("📤 Sending test message to report_tasks...");

            var factory = new ConnectionFactory {
                HostName = "localhost"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "report_tasks",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = new ReportMessage {
                ChatId = 123456789,
                CompanyName = "Test Corp",
                LoanAmount = 100000,
                TotalInterest = 3450,
                TotalPayment = 103450,
                Timestamp = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(exchange: "",
                                 routingKey: "report_tasks",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine("✅ Test message sent to report_tasks.");
        }
    }
}
