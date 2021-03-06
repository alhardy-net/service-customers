### Docker

#### Customers API
- container name: service-customers-api
- env vars:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_URLS=http://+
  - AWS_PROFILE={the name of the aws profile to use}
- port bindings
  - containerPort=80
  - hostPort=8000
- command line options
  - ``-network alhardynet -volume /Users/{username}/.aws/:/root/.aws:ro``

#### Customers Worker
- container name: service-customers-worker
- env vars
  - DOTNET_ENVIRONMENT=Development
  - AWS_PROFILE={the name of the aws profile to use}
- port bindings
    - containerPort=80
    - hostPort=8001
- command line options
    - ``-network alhardynet -volume /Users/{username}/.aws/:/root/.aws:ro``


### Local Postgres setup

1. Ensure alhardynet docker bridge network is configured: ``docker network create alhardynet``
2. Pull postgres image: ``docker pull postgres``
3. Create local folder to store postgres data: ``mkdir ${HOME}/postgres-data/``
4. Run postgres container: ``docker run -d --network alhardynet --hostname postgres --name dev-alhardynet-postgres -e POSTGRES_PASSWORD=p@ssword! -v ${HOME}/postgres-data/:/var/lib/postgressql/data -p 5432:5432 postgres``
5. Optional run pgadmin
   1. ``docker pull dpage/pgadmin4``
   2. ``docker run -d -p 8500:80 --network alhardynet --hostname pgadmin4 -e 'PGADMIN_DEFAULT_EMAIL=admin@alhardy.local' -e 'PGADMIN_DEFAULT_PASSWORD=p@ssword!' -v ${HOME}/pgadmin4-data/:/var/lib/pgadmin --name dev-alhardynet-pgadmin4 dpage/pgadmin4``

### Local RabbitMQ setup

1. alhardynet docker bridge network is configured: ``docker network create alhardynet``
2. Pull image: ``docker pull rabbitmq:3-management``
3. Run container: ``docker run -d --network alhardynet -p 15672:15672 -p 5672:5672 --name dev-alhardynet-rabbitmq rabbitmq:3-management``
4. Management UI: open http://localhost:15672 un: guest pw: guest

### Local distributed tracing setup

Using the open telemetry collector and zipkin

1. alhardynet docker bridge network is configured: ``docker network create alhardynet``
2. docker run --rm --network alhardynet -p 4317:4317 -v "${PWD}"/:/config --name otelcol otel/opentelemetry-collector:latest --config config/otel-collector-config-local.yml --log-level debug
3. docker run --rm --network alhardynet -p 9411:9411 --name zipkin openzipkin/zipkin:latest
4. Zipkin UI: open http://localhost:9411/

### Migration Scripts

``cd src/Customers.Persistence``

``dotnet ef migrations script --idempotent --output MigrationScripts/{script-name}.sql``