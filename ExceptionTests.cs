using NUnit.Framework; // Importa a biblioteca NUnit para testes.
using OpenQA.Selenium; // Importa a biblioteca Selenium para automação de testes web.
using OpenQA.Selenium.Chrome; // Importa o driver do Chrome para Selenium.
using SeleniumExtras.WaitHelpers; // Importa helpers para esperas no Selenium.
using OpenQA.Selenium.Support.UI; // Importa suporte para WebDriverWait.
using WebDriverManager; // Importa o WebDriverManager para gerenciar drivers.
using WebDriverManager.DriverConfigs.Impl; // Importa configurações do WebDriverManager.
using FluentAssertions; // Importa FluentAssertions para asserções em testes.
using System.Linq; // Importa Linq para manipulação de coleções.
using System.Collections.Generic; // Importa coleções genéricas.
using System.Configuration; // Importa para acessar configurações.
using System; // Importa para uso de funcionalidades básicas.

namespace PortalTests // Namespace para agrupar testes.
{
    [TestFixture] // Indica que esta classe contém testes.
    public class ExceptionTests
    {
        private IWebDriver driver; // Declara o driver do Selenium.
        private WebDriverWait wait; // Declara um objeto para gerenciar esperas.
        private NotaFiscalHelper notaFiscalHelper; // Declara um helper para operações de Nota Fiscal.

        [SetUp] // Método que é executado antes de cada teste.
        public void Setup()
        {
            // Configura opções para o ChromeDriver.
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized"); // Inicia o navegador maximizado.

            // Configura o driver do Chrome utilizando o WebDriverManager.
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver(options); // Inicializa o ChromeDriver.
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); // Configura espera de 10 segundos.
            notaFiscalHelper = new NotaFiscalHelper(driver); // Inicializa o helper de Nota Fiscal.
        }

        [Test] // Indica que este método é um teste.
        public void AllEmptyFieldAlerts()
        {
            // Navega até a URL específica do formulário.
            driver.Navigate().GoToUrl("URL_DO_FORMULARIO"); // Insira a URL do formulário aqui.

            // Chama métodos para liberar envio e submeter o formulário.
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();

            // Lista de mensagens de erro esperadas para campos vazios.
            List<string> expectedErrors = new List<string>
            {
                "Preencha \"EMAIL SETOR FISCAL\".",
                "Preencha \"CNPJ\".",
                "Preencha \"NOME DO CONTATO\".",
                "Preencha \"TELEFONE DE CONTATO\".",
                "Preencha \"NÚMERO DA NOTA\".",
                "Preencha \"TELEFONE SETOR FISCAL\".",
                "Preencha \"VALOR DA NOTA\"."
            };

            // Espera até que as mensagens de erro estejam visíveis e armazena na lista.
            var errorMessages = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Preencha')]")));
            List<string> actualErrors = errorMessages.Select(element => element.Text).ToList();

            // Verifica se todas as mensagens de erro esperadas estão presentes.
            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().Contain(expectedError, $"A mensagem '{expectedError}' não foi encontrada.");
            }
        }

        [Test]
        public void NoteAlreadySent()
        {
            // Navega para a URL onde a nota já foi enviada.
            driver.Navigate().GoToUrl("URL_DA_NOTA_JA_ENVIADA"); // Insira a URL aqui.
            var errorMessageElement = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!')]"))).FirstOrDefault();

            // Se a mensagem de erro é encontrada, valida seu conteúdo.
            if (errorMessageElement != null)
            {
                var errorMessage = errorMessageElement.Text;
                errorMessage.Should().Contain("Nota Fiscal já encaminhada, não é possível encaminha-la novamente!", "A mensagem 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!' não foi encontrada.");
            }
            else
            {
                // Caso a mensagem não seja encontrada, preenche o formulário e submete.
                notaFiscalHelper.UpdateDates();
                notaFiscalHelper.Fill1322();
                notaFiscalHelper.ReturnDates();
                notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
                notaFiscalHelper.LiberarParaEnvio();
                notaFiscalHelper.Submit();

                // Espera pela mensagem de sucesso após o envio.
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota enviada com sucesso!')]")));
                driver.PageSource.Should().Contain("Nota enviada com sucesso!", "A mensagem 'Nota enviada com sucesso!' não foi encontrada.");

                // Navega novamente para verificar se a nota ainda está enviada.
                driver.Navigate().GoToUrl("URL_DA_NOTA_JA_ENVIADA"); // Insira a URL novamente aqui.
                errorMessageElement = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!')]"))).FirstOrDefault();
                var errorMessage = errorMessageElement?.Text;
                errorMessage.Should().Contain("Nota Fiscal já encaminhada, não é possível encaminha-la novamente!", "A mensagem 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!' não foi encontrada.");
            }
        }

        [Test]
        public void WrongFileInXmlAndPdfFields()
        {
            // Navega para a URL do formulário de Nota Fiscal.
            driver.Navigate().GoToUrl("URL_DO_FORMULARIO"); // Insira a URL do formulário aqui.

            // Envia arquivos de PDF e XML inválidos para os campos correspondentes.
            driver.FindElement(By.Id("Nota_xml")).SendKeys("CAMINHO_DO_ARQUIVO_PDF"); // Insira o caminho do arquivo PDF aqui.
            driver.FindElement(By.Id("Nota_pdf")).SendKeys("CAMINHO_DO_ARQUIVO_XML"); // Insira o caminho do arquivo XML aqui.

            // Espera até que as mensagens de erro apropriadas sejam visíveis.
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(text(), 'O arquivo inserido deve ser um XML!') or contains(text(), 'O arquivo inserido deve ser um PDF!')]")));
            notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();

            // Coleta mensagens de erro geradas e valida se estão corretas.
            var errorMessages = driver.FindElements(By.XPath("//*[contains(text(), 'O arquivo inserido deve ser um XML!') or contains(text(), 'O arquivo inserido deve ser um PDF!')]"));
            List<string> actualErrors = errorMessages.Select(element => element.Text).ToList();
            List<string> expectedErrors = new List<string>
            {
                "O arquivo inserido deve ser um XML!",
                "O arquivo inserido deve ser um PDF!"
            };

            // Verifica se todas as mensagens de erro esperadas estão presentes.
            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().Contain(expectedError, $"A mensagem '{expectedError}' não foi encontrada.");
            }
        }

        [Test]
        public void NoDocumentInPDFAndXMLFields()
        {
            // Navega até a URL onde o formulário é acessado.
            driver.Navigate().GoToUrl("URL_DO_FORMULARIO"); // Insira a URL do formulário aqui.
            notaFiscalHelper.UpdateDates(); // Atualiza as datas do formulário.
            notaFiscalHelper.Fill3065(); // Preenche o formulário com dados específicos.
            notaFiscalHelper.LiberarParaEnvio(); // Prepara o formulário para envio.
            notaFiscalHelper.Submit(); // Submete o formulário.

            // Espera até que as mensagens de erro apropriadas sejam visíveis.
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(text(), 'Campo requerido!')]")));
            var errorMessages = driver.FindElements(By.XPath("//*[contains(text(), 'Campo requerido!')]"));
            List<string> actualErrors = errorMessages.Select(element => element.Text).ToList();
            List<string> expectedErrors = new List<string>
            {
                "Campo requerido!"
            };

            // Verifica se todas as mensagens de erro esperadas estão presentes.
            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().Contain(expectedError, $"A mensagem '{expectedError}' não foi encontrada.");
            }
        }

        [TearDown] // Método que é executado após cada teste.
        public void TearDown()
        {
            driver.Quit(); // Fecha o navegador após os testes.
        }
    }
}
