# Requires:

Docker Desktop

.NET 8.0 SDK

Python (only if you need to run update-databases.py)

## Run
run command: 

```sh
docker-compose up --build
```

### To update databases

download dotnet ef tool by running command:

```sh
dotnet tool install --global dotnet-ef
```

run command: 

```sh
python update-databases.py
```
