using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System;

//
//
//Driver não atualiza conforme o Chrome
//
//
namespace PortalTests
{
    [TestFixture]
    public class ExceptionTests
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private NotaFiscalHelper notaFiscalHelper;

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            notaFiscalHelper = new NotaFiscalHelper(driver);
        }

        [Test]
        public void AllEmptyFieldAlerts()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedorTeste/Client/notafiscal.html?");
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();
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
            var errorMessages = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Preencha')]")));
            List<string> actualErrors = errorMessages.Select(element => element.Text).ToList();
            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().Contain(expectedError, $"A mensagem '{expectedError}' não foi encontrada.");
            }
        }

        //Ajustar os caminhos dos documentos para o Else
        [Test]
        public void NoteAlreadySent()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?ee48e64680d085864cb1a0e3aa6bf5c5");
            var errorMessageElement = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!')]"))).FirstOrDefault();

            if (errorMessageElement != null)
            {
                var errorMessage = errorMessageElement.Text;
                errorMessage.Should().Contain("Nota Fiscal já encaminhada, não é possível encaminha-la novamente!", "A mensagem 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!' não foi encontrada.");
            }
            else
            {
                notaFiscalHelper.UpdateDates();
                notaFiscalHelper.Fill1322();
                notaFiscalHelper.ReturnDates();
                notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
                notaFiscalHelper.LiberarParaEnvio();
                notaFiscalHelper.Submit();
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota enviada com sucesso!')]")));
                driver.PageSource.Should().Contain("Nota enviada com sucesso!", "A mensagem 'Nota enviada com sucesso!' não foi encontrada.");
                driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?ee48e64680d085864cb1a0e3aa6bf5c5");
                errorMessageElement = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!')]"))).FirstOrDefault();
                var errorMessage = errorMessageElement?.Text;
                errorMessage.Should().Contain("Nota Fiscal já encaminhada, não é possível encaminha-la novamente!", "A mensagem 'Nota Fiscal já encaminhada, não é possível encaminha-la novamente!' não foi encontrada.");
            }
        }


        [Test]
        public void WrongFileInXmlAndPdfFields()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?695d9a00c3fe0f8d7392537b6a805cdf");
            driver.FindElement(By.Id("Nota_xml")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NF 3123 ASSOC HOSP MOINHOS DE VENTO.PDF");
            driver.FindElement(By.Id("Nota_pdf")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NFSE_RPS_956500169_20231025_20231025.XML");
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(text(), 'O arquivo inserido deve ser um XML!') or contains(text(), 'O arquivo inserido deve ser um PDF!')]")));
            notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();
            var errorMessages = driver.FindElements(By.XPath("//*[contains(text(), 'O arquivo inserido deve ser um XML!') or contains(text(), 'O arquivo inserido deve ser um PDF!')]"));
            List<string> actualErrors = errorMessages.Select(element => element.Text).ToList();
            List<string> expectedErrors = new List<string>
    {
        "O arquivo inserido deve ser um XML!",
        "O arquivo inserido deve ser um PDF!"
    };
            foreach (var expectedError in expectedErrors)
            {
                actualErrors.Should().Contain(expectedError, $"A mensagem '{expectedError}' não foi encontrada.");
            }
        }

        [Test]
        public void NoDocumentInPDFAndXMLFields()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?793051a31ef93ca2bab16c34e7fd7d47");
            notaFiscalHelper.UpdateDates();
            notaFiscalHelper.Fill3065();
            notaFiscalHelper.ReturnDates();
            notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
            notaFiscalHelper.Submit();
            var errorMessage = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Documento não preenchido,')]"))).FirstOrDefault()?.Text;
            errorMessage.Should().Contain("Documento não preenchido,", "A mensagem 'Documento não preenchido,' não foi encontrada.");
        }

        [Test]
        public void LibereParaOEnvioButtonUnpressed()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?793051a31ef93ca2bab16c34e7fd7d47");
            notaFiscalHelper.UpdateDates();
            notaFiscalHelper.Fill3065();
            notaFiscalHelper.ReturnDates();
            driver.FindElement(By.Id("Nota_pdf")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NF 3123 ASSOC HOSP MOINHOS DE VENTO.PDF");
            driver.FindElement(By.Id("Nota_xml")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NFSE_RPS_956500169_20231025_20231025.XML");
            notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
            notaFiscalHelper.Submit();
            var errorMessage = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Libere para o envio,')]"))).FirstOrDefault()?.Text;
            errorMessage.Should().Contain("Libere para o envio,", "A mensagem 'Libere para o envio,' não foi encontrada.");
            
        }

        //Entender o porque essa nota parece errada
        [Test]
        public void WrongValueInXML()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?793051a31ef93ca2bab16c34e7fd7d47");
            notaFiscalHelper.UpdateDates();
            notaFiscalHelper.Fill3065();
            notaFiscalHelper.ReturnDates();
            driver.FindElement(By.Id("Nota_pdf")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16237982_NF Nº 8232 - ASSOCIAÇÃO HOSPITALAR MOINHOS DE VENTO.PDF");
            driver.FindElement(By.Id("Nota_xml")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16237982_8232 (wrongValue).XML");
            Thread.Sleep(5000);
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();
            Thread.Sleep(10000);
        }

        [Test]
        public void WrongCNPJInXML()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?793051a31ef93ca2bab16c34e7fd7d47");
            notaFiscalHelper.UpdateDates();
            notaFiscalHelper.Fill3065();
            notaFiscalHelper.ReturnDates();
            driver.FindElement(By.Id("Nota_pdf")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NF 3123 ASSOC HOSP MOINHOS DE VENTO.PDF");
            driver.FindElement(By.Id("Nota_xml")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NFSE_RPS_956500169_20231025_20231025.XML");
            notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();
            var errorMessage = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'CNPJ do prestador não identificado no Arquivo')]"))).FirstOrDefault()?.Text;
            errorMessage.Should().Contain("CNPJ do prestador não identificado no Arquivo", "A mensagem 'CNPJ do prestador não identificado no Arquivo' não foi encontrada.");
        }
        
[Test]
public void InvalidDateField()
{
    driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?793051a31ef93ca2bab16c34e7fd7d47");
    notaFiscalHelper.UpdateDates();
    notaFiscalHelper.Fill3065();
    notaFiscalHelper.ReturnDates();
    driver.FindElement(By.Id("Emissao")).Clear();
    driver.FindElement(By.Id("Emissao")).SendKeys("25022024");
    driver.FindElement(By.Id("Nota_pdf")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NF 3123 ASSOC HOSP MOINHOS DE VENTO.PDF");
    driver.FindElement(By.Id("Nota_xml")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16004996_NFSE_RPS_956500169_20231025_20231025.XML");
    notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
    notaFiscalHelper.LiberarParaEnvio();
    notaFiscalHelper.Submit();
    try
    {
         var errorMessage = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Prezado Usuário, o envio de notas não é mais permitido, pois passou do periodo de 4 dias uteis após a emissão')]"))).FirstOrDefault()?.Text;
                errorMessage.Should().Contain("Prezado Usuário, o envio de notas não é mais permitido, pois passou do periodo de 4 dias uteis após a emissão", "A mensagem 'Prezado Usuário, o envio de notas não é mais permitido, pois passou do periodo de 4 dias uteis após a emissão' não foi encontrada.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exceção ao tentar pegar a mensagem de erro: " + ex.Message);
    }
}


        [Test]
        public void XMLNotFromHMV()
        {
            driver.Navigate().GoToUrl("https://demo.powerserv.com.br/osfornecedor/Client/notafiscal.html?695d9a00c3fe0f8d7392537b6a805cdf");
            notaFiscalHelper.UpdateDates();
            notaFiscalHelper.Fill3065();
            notaFiscalHelper.ReturnDates();
            driver.FindElement(By.Id("Nota_pdf")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\3065_16237982_NF Nº 8232 - ASSOCIAÇÃO HOSPITALAR MOINHOS DE VENTO.PDF");
            driver.FindElement(By.Id("Nota_xml")).SendKeys("C:\\Users\\pedro.lima\\Desktop\\TestesFormPortal\\ExceptionFiles\\The Great Gatsby.xml");
            notaFiscalHelper.WaitForCnpjFieldToBeFilled(driver);
            notaFiscalHelper.LiberarParaEnvio();
            notaFiscalHelper.Submit();
            Thread.Sleep(10000);
            //Pegar o nome do erro corretamente
            //var errorMessage = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[contains(text(), 'Libere para o envio,')]"))).FirstOrDefault()?.Text;
            //errorMessage.Should().Contain("Libere para o envio,", "A mensagem 'Libere para o envio,' não foi encontrada.");

        }

        [TearDown]
        public void Teardown()
        {
            driver.Quit();
        }
    }
}
