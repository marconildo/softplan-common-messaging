using System.Threading.Tasks;

namespace Softplan.Common.Messaging.Abstractions
{
    public interface IPublisher
    {
        /// <summary>
        /// Publica a mensagem no broker.
        /// Caso destination não seja informado será utilizada a informação de destino presente na mensagem.
        /// </summary>
        /// <param name="message">Mensagem a ser publicada no Broker</param>
        /// <param name="destination">Fila de destino para a mensagem</param>
        /// <param name="forceDestination">Caso seja True, irá ignorar informações de destino presentes na Mensagem</param>
        void Publish(IMessage message, string destination = "", bool forceDestination = false);

        /// <summary>
        /// Publica a mensagem no broker e aguarda a resposta.
        /// Caso destination não seja informado será utilizada a informação de destino presente na mensagem.
        /// </summary>
        /// <param name="message">Mensagem a ser publicada no Broker</param>
        /// <param name="destination">Fila de destino para a mensagem</param>
        /// <param name="forceDestination">Caso seja True, irá ignorar informações de destino presentes na Mensagem</param>
        /// <param name="milliSecondsTimeout">Tempo máximo para aguarda a resposta.</param>
        /// <returns>Retorna a Mensagem de resposta da operação publicada</returns>
        Task<T> PublishAndWait<T>(IMessage message, string destination = "", bool forceDestination = false, int milliSecondsTimeout = 60000) where T : IMessage;
    }
}
