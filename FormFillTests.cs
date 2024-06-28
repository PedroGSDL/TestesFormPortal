using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

//
//
//Revisar Waits (talvez adicionar o wait para o preenchimento do CNPJ
//
//
namespace PortalTests
{
    public class FormFillTests
    {
        private IWebDriver driver;
        private HttpClient client;

        [SetUp]
        public void Setup()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            ChromeOptions options = new ChromeOptions();
            driver = new ChromeDriver(options);
            client = new HttpClient();
        }

        [Test, TestCaseSource(typeof(FormHelper), nameof(FormHelper.Files))]
        public async Task FormFill((string json, string pdf, string xml) files)
        {
            JObject formData = JObject.Parse(File.ReadAllText(files.json));
            FormHelper formHelper = new FormHelper(driver);
            FormHelper.UpdateDateInXml(files.xml, false);
            await formHelper.FillFormAndSubmit(formData, files.pdf, files.xml);
            FormHelper.UpdateDateInXml(files.xml, true);
        }

        [TearDown]
        public void CleanUp()
        {
            driver.Quit();
            client.Dispose();
        }
    }
}
