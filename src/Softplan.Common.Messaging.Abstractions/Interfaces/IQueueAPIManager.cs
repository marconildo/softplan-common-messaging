namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IQueueApiManager
    {
        /// <summary>
        /// Retorna dados da fila no Broker
        /// </summary>
        /// <param name="queueName">Nome da fila</param>
        /// <returns>Dados da fila no broker</returns>
        QueueInfo GetQueueInfo(string queueName);

        /// <summary>
        /// Valida se a fila desejada existe e retorna o nome da fila no Broker
        /// </summary>
        /// <param name="queueName">Nome da fila que se deseja verificar</param>
        /// <returns>Nome da fila existente no Broker</returns>
        string EnsureQueue(string queueName);
    }
}
