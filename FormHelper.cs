using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PortalTests
{
    public class FormHelper
    {
        private readonly IWebDriver driver;
        private readonly HttpClient client;
        private readonly WebDriverWait wait;

        // Construtor da classe FormHelper
        public FormHelper(IWebDriver driver)
        {
            this.driver = driver;
            this.client = new HttpClient();
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
        }

        private static Dictionary<string, List<string>> originalValues = new Dictionary<string, List<string>>();

        public static void UpdateDateInXml(string filePath, bool restore)
        {
            XDocument xmlDoc = XDocument.Load(filePath);
            var dateTags = new string[] { "DataEmissaoNFE", "Competencia", "DataEmissaoRPS", "DataEmissao", "DhProc", "DhEmi", "Compet", "DataEmissaoNFe" };

            if (restore)
            {
                // Restore original values
                foreach (var tagName in dateTags)
                {
                    if (originalValues.ContainsKey(tagName))
                    {
                        var dateElements = xmlDoc.Descendants().Where(e => e.Name.LocalName == tagName).ToList();
                        var originalValuesList = originalValues[tagName];

                        for (int i = 0; i < dateElements.Count; i++)
                        {
                            dateElements[i].Value = originalValuesList[i];
                        }
                    }
                }
            }
            else
            {
                // Store original values and update dates
                foreach (var tagName in dateTags)
                {
                    var dateElements = xmlDoc.Descendants().Where(e => e.Name.LocalName == tagName).ToList();
                    var originalValuesList = new List<string>();

                    foreach (var dateElement in dateElements)
                    {
                        originalValuesList.Add(dateElement.Value);

                        DateTime currentDate = DateTime.Now;
                        DateTime newDate;

                        if (currentDate.Day >= 26)
                        {
                            newDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1);
                        }
                        else
                        {
                            newDate = currentDate;
                        }

                        dateElement.Value = newDate.ToString("yyyy-MM-dd");
                    }

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

            xmlDoc.Save(filePath);
        }

        // Método estático para obter os caminhos dos arquivos JSON, PDF e XML
        public static IEnumerable<(string json, string pdf, string xml)> Files()
        {
            string folderPath = @"C:\Users\pedro.lima\Desktop\TestesFormPortal\XML, JSON e PDF\";

            // Itera sobre os arquivos .json na pasta e subpastas
            foreach (var jsonFile in Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories))
            {
                var jsonFileName = Path.GetFileNameWithoutExtension(jsonFile);
                var baseFileName = jsonFileName.Split('#')[0];

                var pdfFilePattern = $"{baseFileName}_*.pdf";
                var xmlFilePattern = $"{baseFileName}_*.xml";

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
            // Dados do formulário em formato JSON
            var requestData = new
            {
                cnpj = (string)formData["cnpj"],
                solicitacao = (string)formData["solicitacao"],
                fornecedor_email = (string)formData["fornecedor_email"],
                razaosocial = (string)formData["razaosocial"],
                solicitante_email = (string)formData["solicitante_email"],
                nota_valor = (string)formData["nota_valor"],
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await client.PostAsync("https://demo.powerserv.com.br/osfornecedor/api/NotaFiscal", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao chamar a API: {ex.Message}");
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao chamar a API: {response.StatusCode}");
                Console.WriteLine($"Detalhes do erro: {errorResponse}");
                throw new Exception($"Erro ao chamar a API: {response.StatusCode}");
            }

            var url = await response.Content.ReadAsStringAsync();
            Console.WriteLine(url);

            DateTime currentDate = DateTime.Now;
            DateTime newDate;

            if (currentDate.Day >= 26)
            {
                newDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1);
            }
            else
            {
                newDate = currentDate;
            }

            driver.Navigate().GoToUrl(url);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Contato_nome")));

            driver.FindElement(By.Id("Emissao")).SendKeys(newDate.ToString("dd/MM/yyyy"));
            driver.FindElement(By.Id("Contato_nome")).SendKeys((string)formData["Nota_os_NomedeContato"]);
            driver.FindElement(By.Id("SetorFiscal_email")).SendKeys((string)formData["Nota_os_emailSetorFiscal"]);
            driver.FindElement(By.Id("Contato_telefone")).SendKeys((string)formData["Nota_os_telefone"]);
            driver.FindElement(By.Id("SetorFiscal_telefone")).SendKeys((string)formData["Nota_os_TelefoneSetorFiscal"]);
            driver.FindElement(By.Id("Nota_numero")).SendKeys((string)formData["Nota_os_NumeroNota"]);
            driver.FindElement(By.Id("Nota_pdf")).SendKeys(pdfPath);
            driver.FindElement(By.Id("Nota_xml")).SendKeys(xmlPath);

            Thread.Sleep(6000);

            driver.FindElement(By.Id("LiberarParaEnvio_input")).Click();
            driver.FindElement(By.Name("submit")).Click();
            wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota enviada com sucesso!')]")));
            driver.PageSource.Should().Contain("Nota enviada com sucesso!", "A mensagem 'Nota enviada com sucesso!' não foi encontrada.");
        }
    }
}