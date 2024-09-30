using OpenQA.Selenium; // Importa a biblioteca do Selenium para automação de navegadores
using OpenQA.Selenium.Support.UI; // Importa suporte para operações de espera
using SeleniumExtras.WaitHelpers; // Importa métodos de espera específicos do Selenium
using FluentAssertions; // Importa a biblioteca FluentAssertions para asserções em testes
using Newtonsoft.Json.Linq; // Importa o Newtonsoft.Json para manipulação de JSON
using Newtonsoft.Json; // Importa o Newtonsoft.Json para serialização e desserialização de JSON
using System.Text; // Importa classes para manipulação de texto
using System.Net.Http; // Importa classes para fazer requisições HTTP
using System.Collections.Generic; // Importa classes para listas e dicionários
using System.IO; // Importa classes para manipulação de arquivos
using System; // Importa classes básicas do .NET
using System.Threading; // Importa classes para controle de threads
using System.Threading.Tasks; // Importa classes para programação assíncrona
using System.Xml.Linq; // Importa classes para manipulação de XML

namespace PortalTests // Namespace do projeto de testes
{
    public class FormHelper // Classe auxiliar para interações com formulários
    {
        private readonly IWebDriver driver; // Driver do Selenium para interagir com o navegador
        private readonly HttpClient client; // Cliente HTTP para fazer requisições
        private readonly WebDriverWait wait; // Objeto para gerenciar tempos de espera no Selenium

        // Construtor da classe FormHelper
        public FormHelper(IWebDriver driver)
        {
            this.driver = driver; // Inicializa o driver
            this.client = new HttpClient(); // Inicializa o cliente HTTP
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60)); // Inicializa o objeto de espera com timeout de 60 segundos
        }

        // Dicionário para armazenar valores originais de tags de data
        private static Dictionary<string, List<string>> originalValues = new Dictionary<string, List<string>>();

        // Método para atualizar datas em um arquivo XML
        public static void UpdateDateInXml(string filePath, bool restore)
        {
            // Carrega o documento XML
            XDocument xmlDoc = XDocument.Load(filePath);
            // Define as tags de data que serão atualizadas
            var dateTags = new string[] { "DataEmissaoNFE", "Competencia", "DataEmissaoRPS", "DataEmissao", "DhProc", "DhEmi", "Compet", "DataEmissaoNFe" };

            if (restore)
            {
                // Restaura os valores originais das datas
                foreach (var tagName in dateTags)
                {
                    if (originalValues.ContainsKey(tagName))
                    {
                        // Encontra os elementos XML com a tag correspondente
                        var dateElements = xmlDoc.Descendants().Where(e => e.Name.LocalName == tagName).ToList();
                        var originalValuesList = originalValues[tagName];

                        for (int i = 0; i < dateElements.Count; i++)
                        {
                            // Restaura o valor original
                            dateElements[i].Value = originalValuesList[i];
                        }
                    }
                }
            }
            else
            {
                // Armazena os valores originais e atualiza as datas
                foreach (var tagName in dateTags)
                {
                    var dateElements = xmlDoc.Descendants().Where(e => e.Name.LocalName == tagName).ToList();
                    var originalValuesList = new List<string>();

                    foreach (var dateElement in dateElements)
                    {
                        // Armazena o valor original
                        originalValuesList.Add(dateElement.Value);

                        DateTime currentDate = DateTime.Now; // Obtém a data atual
                        DateTime newDate;

                        // Lógica para definir nova data dependendo do dia do mês
                        if (currentDate.Day >= 26)
                        {
                            newDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1); // Se dia >= 26, vai para o primeiro dia do próximo mês
                        }
                        else
                        {
                            newDate = currentDate; // Caso contrário, mantém a data atual
                        }

                        // Atualiza o elemento XML com a nova data
                        dateElement.Value = newDate.ToString("yyyy-MM-dd");
                    }

                    // Armazena os valores originais no dicionário
                    if (originalValues.ContainsKey(tagName))
                    {
                        originalValues[tagName] = originalValuesList;
                    }
                    else
                    {
                        originalValues.Add(tagName, originalValuesList);
                    }
                }
            }

            // Salva as alterações no arquivo XML
            xmlDoc.Save(filePath);
        }

        // Método estático para obter os caminhos dos arquivos JSON, PDF e XML
        public static IEnumerable<(string json, string pdf, string xml)> Files()
        {
            // Use uma variável de ambiente ou um arquivo de configuração para o caminho
            string folderPath = Environment.GetEnvironmentVariable("TEST_FOLDER_PATH") ?? @"C:\Path\To\Your\TestFolder\";

            // Itera sobre os arquivos .json na pasta e subpastas
            foreach (var jsonFile in Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories))
            {
                var jsonFileName = Path.GetFileNameWithoutExtension(jsonFile);
                var baseFileName = jsonFileName.Split('#')[0];

                // Define padrões para arquivos PDF e XML correspondentes
                var pdfFilePattern = $"{baseFileName}_*.pdf";
                var xmlFilePattern = $"{baseFileName}_*.xml";

                // Busca arquivos PDF e XML na pasta
                var pdfFiles = Directory.GetFiles(folderPath, pdfFilePattern, SearchOption.AllDirectories);
                var xmlFiles = Directory.GetFiles(folderPath, xmlFilePattern, SearchOption.AllDirectories);

                // Verifica se há pelo menos um arquivo PDF e um arquivo XML correspondentes ao arquivo JSON
                if (pdfFiles.Length > 0 && xmlFiles.Length > 0)
                {
                    yield return (jsonFile, pdfFiles[0], xmlFiles[0]); // Retorna os caminhos dos arquivos encontrados
                }
            }
        }

        // Método assíncrono para preencher o formulário e submetê-lo
        public async Task FillFormAndSubmit(JObject formData, string pdfPath, string xmlPath)
        {
            // Dados do formulário em formato JSON, utilizando placeholders para dados sensíveis
            var requestData = new
            {
                cnpj = (string)formData["cnpj"], // CNPJ deve ser mascarado se exibido
                solicitacao = (string)formData["solicitacao"],
                fornecedor_email = (string)formData["fornecedor_email"],
                razaosocial = (string)formData["razaosocial"],
                solicitante_email = (string)formData["solicitante_email"],
                nota_valor = (string)formData["nota_valor"],
            };

            // Serializa os dados do formulário para JSON
            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null; // Resposta da requisição HTTP
            try
            {
                // Envia a requisição para a API
                response = await client.PostAsync("https://demo.powerserv.com.br/osfornecedor/api/NotaFiscal", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao chamar a API: {ex.Message}"); // Evita expor detalhes sensíveis
                throw; // Lança a exceção para tratamento externo
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao chamar a API: {response.StatusCode}"); // Loga o status da resposta
                Console.WriteLine($"Detalhes do erro: {errorResponse}"); // Considere não expor informações sensíveis em logs
                throw new Exception($"Erro ao chamar a API: {response.StatusCode}"); // Lança exceção com status de erro
            }

            var url = await response.Content.ReadAsStringAsync(); // Obtém a URL da resposta
            Console.WriteLine(url); // Loga a URL

            DateTime currentDate = DateTime.Now; // Obtém a data atual
            DateTime newDate;

            // Lógica para definir nova data dependendo do dia do mês
            if (currentDate.Day >= 26)
            {
                newDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1); // Se dia >= 26, vai para o primeiro dia do próximo mês
            }
            else
            {
                newDate = currentDate; // Caso contrário, mantém a data atual
            }

            driver.Navigate().GoToUrl(url); // Navega para a URL retornada pela API
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Contato_nome"))); // Aguarda até que o campo de nome do contato esteja visível

            // Preenche os campos do formulário com os dados fornecidos
            driver.FindElement(By.Id("Emissao")).SendKeys(newDate.ToString("dd/MM/yyyy")); // Preenche a data de emissão
            driver.FindElement(By.Id("Contato_nome")).SendKeys((string)formData["Nota_os_NomedeContato"]); // Preenche o nome do contato
            driver.FindElement(By.Id("SetorFiscal_email")).SendKeys((string)formData["Nota_os_emailSetorFiscal"]); // Preenche o e-mail do setor fiscal
            driver.FindElement(By.Id("Contato_telefone")).SendKeys((string)formData["Nota_os_telefone"]); // Preenche o telefone do contato
            driver.FindElement(By.Id("SetorFiscal_telefone")).SendKeys((string)formData["Nota_os_TelefoneSetorFiscal"]); // Preenche o telefone do setor fiscal
            driver.FindElement(By.Id("Nota_numero")).SendKeys((string)formData["Nota_os_NumeroNota"]); // Preenche o número da nota
            driver.FindElement(By.Id("Nota_pdf")).SendKeys(pdfPath); // Envia o caminho do arquivo PDF
            driver.FindElement(By.Id("Nota_xml")).SendKeys(xmlPath); // Envia o caminho do arquivo XML

            Thread.Sleep(6000); // Aguarda 6 segundos (pode ser ajustado para uma espera mais adequada)

            // Realiza a submissão do formulário
            driver.FindElement(By.Id("LiberarParaEnvio_input")).Click(); // Marca o checkbox para liberar envio
            driver.FindElement(By.Name("submit")).Click(); // Clica no botão de submissão
            wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota enviada com sucesso!')]"))); // Aguarda a mensagem de sucesso

            // Verifica se a mensagem de sucesso foi exibida na página
            driver.PageSource.Should().Contain("Nota enviada com sucesso!", "A mensagem 'Nota enviada com sucesso!' não foi encontrada.");
        }
    }
}
