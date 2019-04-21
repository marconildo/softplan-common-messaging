# Softplan.Common.Messaging

Biblioteca DotNet compatível com o [DelphiMQ](https://git-unj.softplan.com.br/unj-integracoes/DelphiMQ), implementando as mesmas regras e visando facilitar a integração das aplicações legadas com o SAJ 6 e demais aplicações desenvolvidas em C#

## Como usar a lib

Se estiver criando uma WebApi, basta adicionar ao seu `ConfigureServices` uma chamada a `AddMessagingManager`

```csharp
    public void ConfigureServices(IServiceCollection services)
    {

        services.AddMessagingManager(Configuration, loggerFactory);

        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    }
```

E depois iniciar os serviços com o método `StartMessagingManager()` no `Configure` do `Startup.cs`

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
um parametro do tipo `IPublisher` e tem um publisher de mensagens disponível.

O Publisher expõe dois métodos principais, o `Publish` e o `PublishAndWait<T>`, sendo que o primeiro é
uma chamada simples de publicação de mensagem simples, enquanto o segundo publica a mensagem e espera
pela mensagem de resposta do tipo `T`.

### Recebendo mensagens

Todas as implementações da interface `IProcessor` são carregadas automaticamente ao iniciar o serviço
da biblioteca. Esta interface expões quatro métodos:

```csharp
    public interface IProcessor
    {
        string GetQueueName();
        Type GetMessageType();
        void ProcessMessage(IMessage message, IPublisher publisher);
        bool HandleProcessError(IMessage message, IPublisher publisher, Exception error);
    }
```

* **GetQueueName**: Retorna o nome da fila na qual este processador estará inscrito para processar mensagens
* **GetMessageType**: Retorna o `Type` da classe de mensagem para deserialização e processamento
* **ProcessMessage**: O método que efetivamente processa a mensagem recebida
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
