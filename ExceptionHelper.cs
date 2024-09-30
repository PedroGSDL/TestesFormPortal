
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Xml;

public class NotaFiscalHelper
{
    private IWebDriver driver;
    private string originalXmlFilePath;
    private string originalDateValue;
    private string originalCompetenciaValue;
    private string originalDataEmissaoValue;
    private static readonly HttpClient client = new HttpClient();

    public NotaFiscalHelper(IWebDriver driver)
    {
        this.driver = driver;
    }


    public void LiberarParaEnvio()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("LiberarParaEnvio_input")));
        driver.FindElement(By.Id("LiberarParaEnvio_input")).Click();
    }

    public void Submit()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        wait.Until(ExpectedConditions.ElementToBeClickable(By.Name("submit")));
        driver.FindElement(By.Name("submit")).Click();
    }

    public void Fill1322()
    {
        driver.FindElement(By.Id("Contato_nome")).SendKeys("");
        driver.FindElement(By.Id("SetorFiscal_email")).SendKeys("");
        driver.FindElement(By.Id("Contato_telefone")).SendKeys("51");
        driver.FindElement(By.Id("SetorFiscal_telefone")).SendKeys("51");
        driver.FindElement(By.Id("Nota_numero")).SendKeys("");
        driver.FindElement(By.Id("Nota_pdf")).SendKeys("");
        DateTime currentDate = DateTime.Now;
        string formattedDate = currentDate.ToString("dd/MM/yyyy");
        driver.FindElement(By.Id("Emissao")).SendKeys(formattedDate);
        driver.FindElement(By.Id("Nota_xml")).SendKeys("");
    }

    public void Fill3065()
    {
     
        driver.FindElement(By.Id("Contato_nome")).SendKeys("");
        driver.FindElement(By.Id("SetorFiscal_email")).SendKeys("");
        driver.FindElement(By.Id("Contato_telefone")).SendKeys("51");
        driver.FindElement(By.Id("SetorFiscal_telefone")).SendKeys("51");
        driver.FindElement(By.Id("Nota_numero")).SendKeys("3123");
        DateTime currentDate = DateTime.Now;
        string formattedDate = currentDate.ToString("dd/MM/yyyy");
        driver.FindElement(By.Id("Emissao")).SendKeys(formattedDate);
        
    }
  


    public void UpdateDates()
    {
        originalXmlFilePath = @"";
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(originalXmlFilePath);
        XmlNodeList competenciaNodes = xmlDoc.GetElementsByTagName("Competencia");
        if (competenciaNodes.Count > 0)
        {
            originalCompetenciaValue = competenciaNodes[0].InnerText;
        }

        XmlNodeList dataEmissaoNodes = xmlDoc.GetElementsByTagName("DataEmissao");
        if (dataEmissaoNodes.Count > 0)
        {
            originalDataEmissaoValue = dataEmissaoNodes[0].InnerText;
        }
        DateTime currentDate = DateTime.Now;
        string formattedDate = currentDate.ToString("yyyy-MM-dd");
        if (competenciaNodes.Count > 0)
        {
            competenciaNodes[0].InnerText = formattedDate;
        }

        if (dataEmissaoNodes.Count > 0)
        {
            dataEmissaoNodes[0].InnerText = formattedDate;
        }
        xmlDoc.Save(originalXmlFilePath);
        driver.FindElement(By.Id("Nota_xml")).SendKeys(originalXmlFilePath);
    }

    public void WaitForCnpjFieldToBeFilled(IWebDriver driver)
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(driver => !string.IsNullOrEmpty(driver.FindElement(By.Id("Cnpj")).GetAttribute("value")));
    }


    public void ReturnDates()
    {
        if (!string.IsNullOrEmpty(originalXmlFilePath) && File.Exists(originalXmlFilePath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(originalXmlFilePath);
            XmlNodeList competenciaNodes = xmlDoc.GetElementsByTagName("Competencia");
            XmlNodeList dataEmissaoNodes = xmlDoc.GetElementsByTagName("DataEmissao");
            if (competenciaNodes.Count > 0)
            {
                competenciaNodes[0].InnerText = originalCompetenciaValue;
            }

            if (dataEmissaoNodes.Count > 0)
            {
                dataEmissaoNodes[0].InnerText = originalDataEmissaoValue;
            }

            xmlDoc.Save(originalXmlFilePath);
        }
    }
}

