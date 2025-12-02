using Confluent.Kafka;
using System;

Console.WriteLine("=== Confluent Kafka Producer Test ===\n");

// Parse command line arguments
if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    ShowHelp();
    return;
}

string? bootstrapServers = GetArgument(args, "--broker", "-b");
string? username = GetArgument(args, "--username", "-u");
string? password = GetArgument(args, "--password", "-p");

if (string.IsNullOrWhiteSpace(bootstrapServers))
{
    Console.WriteLine("Error: Bootstrap Servers (--broker) is required.");
    Console.WriteLine("Use --help for usage information.");
    return;
}

if (string.IsNullOrWhiteSpace(username))
{
    Console.WriteLine("Error: Username (--username) is required.");
    Console.WriteLine("Use --help for usage information.");
    return;
}

if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Error: Password (--password) is required.");
    Console.WriteLine("Use --help for usage information.");
    return;
}

try
{

    Console.WriteLine("\n--- Creating Producer Configuration ---");
    
    // Create producer configuration
    ProducerConfig? messageProducerConfig = null;
    
    try
    {
        messageProducerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = username,
            SaslPassword = password,
            Acks = Acks.All,
            EnableIdempotence = true,
            CompressionType = CompressionType.Snappy
        };
        
        Console.WriteLine("✓ Producer configuration created successfully");
        Console.WriteLine($"  Bootstrap Servers: {messageProducerConfig.BootstrapServers}");
        Console.WriteLine($"  Security Protocol: {messageProducerConfig.SecurityProtocol}");
        Console.WriteLine($"  SASL Mechanism: {messageProducerConfig.SaslMechanism}");
        Console.WriteLine($"  Username: {messageProducerConfig.SaslUsername}");
        Console.WriteLine($"  Password: {messageProducerConfig.SaslPassword}");
        Console.WriteLine($"  Acks: {messageProducerConfig.Acks}");
        Console.WriteLine($"  Enable Idempotence: {messageProducerConfig.EnableIdempotence}");
        Console.WriteLine($"  Compression Type: {messageProducerConfig.CompressionType}");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"✗ Configuration Error: Invalid configuration parameter");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
        return;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Unexpected Error during configuration: {ex.GetType().Name}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
        return;
    }

    Console.WriteLine("\n--- Building Kafka Producer ---");
    
    // Create and build the producer
    IProducer<string, byte[]>? producer = null;
    
    try
    {
        producer = new ProducerBuilder<string, byte[]>(messageProducerConfig).Build();
        Console.WriteLine("✓ Kafka producer built successfully");
        
        // Test connection by getting metadata (this will trigger actual connection)
        Console.WriteLine("\n--- Testing Connection ---");
        var adminClient = new DependentAdminClientBuilder(producer.Handle).Build();
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        Console.WriteLine($"✓ Successfully connected to Kafka cluster!");
        Console.WriteLine($"  Cluster ID: {metadata.OriginatingBrokerId}");
        Console.WriteLine($"  Brokers: {metadata.Brokers.Count}");
        
        foreach (var broker in metadata.Brokers)
        {
            Console.WriteLine($"    - Broker {broker.BrokerId}: {broker.Host}:{broker.Port}");
        }
        
        Console.WriteLine($"  Topics: {metadata.Topics.Count}");
    }
    catch (ProduceException<string, byte[]> ex)
    {
        Console.WriteLine($"✗ Producer Error: Failed to produce message");
        Console.WriteLine($"  Error Code: {ex.Error.Code}");
        Console.WriteLine($"  Error Reason: {ex.Error.Reason}");
        Console.WriteLine($"  Is Fatal: {ex.Error.IsFatal}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
    }
    catch (KafkaException ex)
    {
        Console.WriteLine($"✗ Kafka Error: {ex.Error.Code}");
        Console.WriteLine($"  Reason: {ex.Error.Reason}");
        Console.WriteLine($"  Is Fatal: {ex.Error.IsFatal}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
        
        // Provide specific guidance based on error code
        switch (ex.Error.Code)
        {
            case ErrorCode.Local_Authentication:
                Console.WriteLine("\n  → Authentication failed. Please verify your username and password.");
                break;
            case ErrorCode.Local_AllBrokersDown:
                Console.WriteLine("\n  → Cannot connect to any brokers. Please verify the bootstrap servers URL.");
                break;
            case ErrorCode.Local_Transport:
                Console.WriteLine("\n  → Network transport error. Check your network connection and firewall settings.");
                break;
            case ErrorCode.SaslAuthenticationFailed:
                Console.WriteLine("\n  → SASL authentication failed. Check your credentials.");
                break;
            case ErrorCode.Local_TimedOut:
                Console.WriteLine("\n  → Connection timed out. The broker may be unreachable or overloaded.");
                break;
            default:
                Console.WriteLine($"\n  → Error Type: {ex.GetType().Name}");
                break;
        }
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"✗ Invalid Operation: {ex.Message}");
        Console.WriteLine("  This may indicate an issue with the producer configuration or state.");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
    }
    catch (System.Net.Sockets.SocketException ex)
    {
        Console.WriteLine($"✗ Network Error: Failed to establish socket connection");
        Console.WriteLine($"  Error Code: {ex.SocketErrorCode}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
        Console.WriteLine("\n  → Please verify the broker URL and ensure the network is accessible.");
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine($"✗ Timeout Error: Operation timed out");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
        Console.WriteLine("\n  → The broker may be unreachable or not responding. Check the broker URL and network connectivity.");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"✗ Authorization Error: Access denied");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
        Console.WriteLine("\n  → Check your credentials and permissions.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Unexpected Error: {ex.GetType().Name}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
    }
    finally
    {
        // Clean up resources
        if (producer != null)
        {
            Console.WriteLine("\n--- Cleaning up ---");
            producer.Flush(TimeSpan.FromSeconds(5));
            producer.Dispose();
            Console.WriteLine("✓ Producer disposed successfully");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Fatal Error: {ex.GetType().Name}");
    Console.WriteLine($"  Message: {ex.Message}");
    Console.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
}

// Helper methods for command line argument parsing
static void ShowHelp()
{
    Console.WriteLine("Usage: KafkaTester --broker <url> --username <user> --password <pass>");
    Console.WriteLine();
    Console.WriteLine("Required Arguments:");
    Console.WriteLine("  --broker, -b <url>       Kafka bootstrap servers (e.g., broker.example.com:9092)");
    Console.WriteLine("  --username, -u <user>    SASL username");
    Console.WriteLine("  --password, -p <pass>    SASL password");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --help, -h              Show this help message");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  KafkaTester --broker pkc-abc.us-east-1.aws.confluent.cloud:9092 --username myuser --password mypass");
}

static string? GetArgument(string[] args, string longName, string shortName)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i].Equals(longName, StringComparison.OrdinalIgnoreCase) || 
            args[i].Equals(shortName, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }
    return null;
}
