### need docker desktop: https://www.docker.com/products/docker-desktop/

### need confluent installed: https://developer.confluent.io/get-started/dotnet/#kafka-setup

### need a lot of patience to debug when this inevitably fails

```
confluent local kafka start
```

```
confluent local kafka topic create purchases
```

in their respective folders:

```
dotnet build producer.csproj
dotnet build consumer.csproj
```

in the consumer folder, then the producer folder folder:

```
dotnet run "$(pwd)/../getting-started.properties"
```

```
confluent local kafka stop
```
