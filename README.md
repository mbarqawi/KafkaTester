# Kafka Tester

A C# console application for testing connections to Confluent Kafka clusters with comprehensive error handling.

## Overview

This application demonstrates how to connect to a Kafka cluster using the Confluent.Kafka client library with SASL/SSL authentication. It prompts for connection credentials, creates a producer, and validates the connection by retrieving cluster metadata.

## Features

- **Interactive User Input**: Prompts for Kafka broker URL, username, and password (with masked input)
- **Secure Authentication**: Supports SASL/SSL with PLAIN mechanism
- **Producer Configuration**: Pre-configured with production-ready settings:
  - Acks: All (ensures highest durability)
  - Enable Idempotence: True (prevents duplicate messages)
  - Compression Type: Snappy
- **Connection Testing**: Validates connectivity by fetching cluster metadata
- **Comprehensive Error Handling**: Catches and provides helpful guidance for:
  - Authentication failures
  - Connection errors
  - Network timeouts
  - SSL/TLS issues
  - Invalid configuration

## Requirements

- .NET 9.0 or later
- Confluent.Kafka NuGet package (v2.3.0)

## Usage

1. Run the application:
   ```bash
   dotnet run
   ```

2. Enter your Kafka cluster details when prompted:
   - **Bootstrap Servers**: Kafka broker address (format: `hostname:port`)
     - Example: `pkc-abc123.us-east-1.aws.confluent.cloud:9092`
   - **Username**: Your SASL username
   - **Password**: Your SASL password (input is masked)

3. The application will:
   - Create and configure the producer
   - Attempt to connect to the cluster
   - Display cluster information if successful
   - Show detailed error messages if connection fails

## Example Output

```
=== Confluent Kafka Producer Test ===

Enter Kafka Broker URL (Bootstrap Servers): broker.example.com:9092
Enter SASL Username: myuser
Enter SASL Password: ********

--- Creating Producer Configuration ---
✓ Producer configuration created successfully
  Bootstrap Servers: broker.example.com:9092
  Security Protocol: SaslSsl
  SASL Mechanism: Plain
  Username: myuser
  Acks: All
  Enable Idempotence: True
  Compression Type: Snappy

--- Building Kafka Producer ---
✓ Kafka producer built successfully

--- Testing Connection ---
✓ Successfully connected to Kafka cluster!
  Cluster ID: 1
  Brokers: 3
    - Broker 1: broker1.example.com:9092
    - Broker 2: broker2.example.com:9092
    - Broker 3: broker3.example.com:9092
  Topics: 10
```

## Configuration

The producer is configured with the following settings:

```csharp
new ProducerConfig
{
    BootstrapServers = "<user input>",
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.Plain,
    SaslUsername = "<user input>",
    SaslPassword = "<user input>",
    Acks = Acks.All,
    EnableIdempotence = true,
    CompressionType = CompressionType.Snappy
};
```

## Error Handling

The application provides specific guidance for common errors:

- **Authentication Failures**: Verify username and password
- **Connection Failures**: Check broker URL and network connectivity
- **Network Timeouts**: Broker may be unreachable or overloaded
- **SSL/TLS Errors**: Verify security settings and certificates
- **Transport Errors**: Check firewall and network settings

## Building

```bash
dotnet build
```

## Dependencies

- [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka/) - Apache Kafka .NET client

---

*This repository was created with GitHub Copilot*
