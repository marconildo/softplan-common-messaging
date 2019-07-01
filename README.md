# Softplan.Common.Messaging

Biblioteca que visa simplificar o uso de padrões comuns nas implementações de softwares que usam recursos
de message brokers como o [RabbitMQ](https://www.rabbitmq.com/).

Esta lib abstrai as partes internas do uso de canais de mensageria e deixa ao desenvolvedor somente a
responsabilidade de escrever a sua lógica sem se preocupar com infra estrutura, convenções e boas
práticas para implementar padrões como PubSub, RequestReply, FireAndForget.

Além disso, a lib esta preparada para a instrumentação com sistemas de APM como o [ElasticAPM](https://www.elastic.co/guide/en/apm/agent/dotnet/current/index.html)

## Como usar a lib

Se estiver criando uma WebApi, basta adicionar ao seu appsettings.json as configurações do brooker.

```csharp
  "MESSAGE_BROKER_URL": "amqp://guest:guest@localhost:5672",
  "MESSAGE_BROKER_API_URL": "http://guest:guest@localhost:15672/api",
  "MESSAGE_BROKER": "RabbitMq"
```

* **MESSAGE_BROKER_URL**: Endereço do broker onde as mensagens serão publicadas.
* **MESSAGE_BROKER_API_URL**: Endereço da API disponibilizada pelo broker.
* **MESSAGE_BROKER**: Tipo de broker, de acordo com `MessageBrokers`.


Adicionar ao seu `ConfigureServices` uma chamada a `AddMessagingManager`.

```csharp
    public void ConfigureServices(IServiceCollection services)
    {

        services.AddMessagingManager(Configuration, loggerFactory);

        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    }
```

E então iniciar os serviços com o método `StartMessagingManager()` no `Configure` do `Startup.cs`

```csharp
    public void Configure(IApplicationBuilder app)
    {
        if (HostingEnvironment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage()
                .UseCors(option => option.AllowAnyOrigin());
            return;
        }

        app.UseHsts().UseHttpsRedirection();

        app.ApplicationServices.StartMessagingManager();
    }
```

### Publicando mensagems

Esta biblioteca adiciona um servico da interface `IPublisher` ao serviço de injeção de dependências
padrão do C#. Com isso, quando estiver em um `WebController` por exemplo, basta adicionar ao contructor
um parametro do tipo `IPublisher` para ter um publisher de mensagens disponível.

```csharp
    public interface IPublisher
    {
        void Publish(IMessage message, string destination = "", bool forceDestination = false);
        Task<T> PublishAndWait<T>(IMessage message, string destination = "", bool forceDestination = false, int milliSecondsTimeout = 60000) where T : IMessage;
    }
```

* **Publish**: Faz uma chamada simples de publicação de mensagem;
* **GetMessageType**: Publica a mensagem e espera
pela mensagem de resposta do tipo `T`.

### Recebendo mensagens

Todas as implementações da interface `IProcessor` são carregadas automaticamente ao iniciar o serviço
da biblioteca. Esta interface expões quatro métodos:

```csharp
    public interface IProcessor
    {
        ILogger Logger { get; set; }
        string GetQueueName();
        Type GetMessageType();
        void ProcessMessage(IMessage message, IPublisher publisher);
        bool HandleProcessError(IMessage message, IPublisher publisher, Exception error);
    }
```

* **Logger**: Classe de logs;
* **GetQueueName**: Retorna o nome da fila na qual este processador estará inscrito para processar mensagens;
* **GetMessageType**: Retorna o `Type` da classe de mensagem para deserialização e processamento;
* **ProcessMessage**: O método que efetivamente processa a mensagem recebida;
* **HandleProcessError**: Método chamado quando ocorrem erros dentro do `ProcessMessage`. Se retornar `true`,
a mensagem será confirmada e retirada da fila. Caso retorne `false` a mensagem é devolvida para a fila para ser
processada novamente.

### Injeção de dependências

A injeção automática de dependências, no mesmo modelo que acontece com os `WebControllers` do Asp.net WebAPI
também pode ser usada nos processadores, alterando o `constructor` dos mesmos para indicar quais são as dependências
necessárias.

Como no exemplo abaixo onde o parametro `config` do tipo `MessagingConfig` é injetado automaticamente quando a
lib inicia.

```csharp
    public class AnaliseBloqueioProcessor : IProcessor
    {
        private readonly MessagingConfig config;

        public AnaliseBloqueioProcessor(MessagingConfig config)
        {
            this.config = config;
        }
    }
```

### Console Applications
É possível usar a biblioteca em aplicações que não sejam Web, porém, para consumir as mensagens, é preciso carregar os `Processors` e iniciar os mesmos.

```csharp
    var loggerFactory = new LoggerFactory();
    var messagingBuilderFactory = new MessagingBuilderFactory();
    var builder = messagingBuilderFactory.GetBuilder(config, loggerFactory);
    using (var manager = new MessagingManager(builder, loggerFactory))
    {
        manager.LoadProcessors(null);
        manager.Start();
        ...
        manager.Stop();                
    }
```

Além disso, é preciso instanciar o `Publisher` para a publicação de mensagens.

```csharp
    var loggerFactory = new LoggerFactory();
    var messagingBuilderFactory = new MessagingBuilderFactory();
    var builder = messagingBuilderFactory.GetBuilder(config, loggerFactory);
    var publisher = builder.BuildPublisher();
```

### Suporte a APM
Para instrumentar a aplicação com APM, basta adicionar as configurações necessárias no appsettings.config.

```csharp
  "APM_PROVIDER": "ElasticApm",
  "APM_TRACE_ASYNC_TRANSACTIONS" : false
```

* **APM_PROVIDER**: Tipo de sistema de APM, de acordo com `ApmProviders`.
* **APM_TRACE_ASYNC_TRANSACTIONS**: Indica se deve manter o trace distribuído de operações asíncronas..

Obs.: A configuração do agente de APM deve ser feita na aplicação, a biblioteca não é responsável por configurar e inicializar o agente.