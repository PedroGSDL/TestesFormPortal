using NUnit.Framework; // Importa o namespace do NUnit para testes unitários.
using OpenQA.Selenium; // Importa o namespace do Selenium para interações com o navegador.
using OpenQA.Selenium.Chrome; // Importa o ChromeDriver do Selenium.
using System; // Importa o namespace básico do .NET.
using System.IO; // Importa para manipulação de arquivos.
using System.Net.Http; // Importa para realizar requisições HTTP.
using System.Threading.Tasks; // Importa para trabalhar com tarefas assíncronas.
using Newtonsoft.Json.Linq; // Importa para manipulação de objetos JSON.
using WebDriverManager; // Importa para gerenciamento de drivers do Selenium.
using WebDriverManager.DriverConfigs.Impl; // Importa configurações de drivers específicas.

namespace PortalTests
{
    public class FormFillTests
    {
        private IWebDriver driver; // Declaração do driver do Selenium.
        private HttpClient client; // Declaração do cliente HTTP.

        // Método que configura o ambiente antes de cada teste.
        [SetUp]
        public void Setup()
        {
            // Configura o driver do Chrome usando o WebDriverManager.
            new DriverManager().SetUpDriver(new ChromeConfig());
            ChromeOptions options = new ChromeOptions(); // Cria opções para o Chrome.
            driver = new ChromeDriver(options); // Inicializa o driver do Chrome com as opções.
            client = new HttpClient(); // Inicializa o cliente HTTP.
        }

        // Método de teste que preenche um formulário, usando dados de arquivos.
        [Test, TestCaseSource(typeof(FormHelper), nameof(FormHelper.Files))]
        public async Task FormFill((string json, string pdf, string xml) files)
        {
            // Lê o arquivo JSON e o converte em um objeto JObject.
            JObject formData = JObject.Parse(File.ReadAllText(files.json));
            FormHelper formHelper = new FormHelper(driver); // Cria uma instância do FormHelper.
            
            // Atualiza a data no arquivo XML para um estado inicial (não preenchido).
            FormHelper.UpdateDateInXml(files.xml, false);
            
            // Preenche o formulário e o envia, usando os dados do JSON e arquivos PDF e XML.
            await formHelper.FillFormAndSubmit(formData, files.pdf, files.xml);
            
            // Atualiza a data no arquivo XML para um estado final (preenchido).
            FormHelper.UpdateDateInXml(files.xml, true);
        }

        // Método que limpa o ambiente após cada teste.
        [TearDown]
        public void CleanUp()
        {
            driver.Quit(); // Fecha o navegador.
            client.Dispose(); // Libera os recursos do cliente HTTP.
        }
    }
}
