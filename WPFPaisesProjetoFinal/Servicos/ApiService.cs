using Newtonsoft.Json;
using System.Net.Http;
using WPFPaisesCExecutavel.Modelos;

namespace WPFPaisesCExecutavel.Servicos
{
    /// <summary>
    /// Classe que fornece métodos para interagir com APIs externas.
    /// </summary>
    public class ApiService
    {
        /// <summary>
        /// Obtém uma lista de países a partir do URL especificado.
        /// </summary>
        /// <param name="url">O URL do endpoint da API para obter os países.</param>
        /// <returns>
        /// Uma tarefa que representa a operação assíncrona. O resultado da tarefa contém uma lista de objetos <see cref="Countries"/> se a operação for bem-sucedida; caso contrário, retorna null.
        /// </returns>
        public async Task<List<Countries>> GetCountries(string url)
        {
            try
            {
                // Cria uma instância de HttpClient para enviar o pedido
                using (HttpClient client = new HttpClient())
                {
                    // Envia um pedido GET para o URL especificado
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verifica se a resposta indica sucesso
                    if (response.IsSuccessStatusCode)
                    {
                        // Lê o conteúdo da resposta como uma string JSON
                        string json = await response.Content.ReadAsStringAsync();

                        // Converte o JSON para uma lista de objetos Countries utilizando o Newtonsoft.Json
                        List<Countries> countries = JsonConvert.DeserializeObject<List<Countries>>(json);

                        // Retorna a lista de países obtida
                        return countries;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
