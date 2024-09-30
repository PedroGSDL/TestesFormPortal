using OpenQA.Selenium; // Importa a biblioteca Selenium para automação de testes web.
using OpenQA.Selenium.Support.UI; // Importa suporte para WebDriverWait.
using SeleniumExtras.WaitHelpers; // Importa helpers para esperas no Selenium.
using System.Xml; // Importa para manipulação de arquivos XML.
using System.Net.Http; // Importa para usar HttpClient.
using System.IO; // Importa para manipulação de arquivos.
using System; // Importa para uso de funcionalidades básicas.

public class NotaFiscalHelper
{
    private IWebDriver driver; // Declara o driver do Selenium.
    private string originalXmlFilePath; // Declara o caminho do arquivo XML original.
    private string originalDateValue; // Declara o valor da data original.
    private string originalCompetenciaValue; // Declara o valor original da competência.
    private string originalDataEmissaoValue; // Declara o valor original da data de emissão.
    private static readonly HttpClient client = new HttpClient(); // Declara um cliente HTTP.

    public NotaFiscalHelper(IWebDriver driver)
    {
        this.driver = driver; // Inicializa o driver do Selenium.
    }

    // Método para liberar o formulário para envio.
    public void LiberarParaEnvio()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); // Configura espera de 10 segundos.
        wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("LiberarParaEnvio_input"))); // Espera até que o elemento esteja clicável.
        driver.FindElement(By.Id("LiberarParaEnvio_input")).Click(); // Clica para liberar o envio.
    }

    // Método para submeter o formulário.
    public void Submit()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20)); // Configura espera de 20 segundos.
        wait.Until(ExpectedConditions.ElementToBeClickable(By.Name("submit"))); // Espera até que o botão de submit esteja clicável.
        driver.FindElement(By.Name("submit")).Click(); // Clica para submeter o formulário.
    }

    // Método para preencher o formulário 1322.
    public void Fill1322()
    {
        driver.FindElement(By.Id("Contato_nome")).SendKeys(""); // Preenche o campo de nome do contato.
        driver.FindElement(By.Id("SetorFiscal_email")).SendKeys(""); // Preenche o campo de e-mail do setor fiscal.
        driver.FindElement(By.Id("Contato_telefone")).SendKeys("51"); // Preenche o telefone do contato.
        driver.FindElement(By.Id("SetorFiscal_telefone")).SendKeys("51"); // Preenche o telefone do setor fiscal.
        driver.FindElement(By.Id("Nota_numero")).SendKeys(""); // Preenche o número da nota.
        driver.FindElement(By.Id("Nota_pdf")).SendKeys(""); // Preenche o campo do PDF da nota.
        
        DateTime currentDate = DateTime.Now; // Obtém a data atual.
        string formattedDate = currentDate.ToString("dd/MM/yyyy"); // Formata a data.
        driver.FindElement(By.Id("Emissao")).SendKeys(formattedDate); // Preenche a data de emissão.
        driver.FindElement(By.Id("Nota_xml")).SendKeys(""); // Preenche o campo do XML da nota.
    }

    // Método para preencher o formulário 3065.
    public void Fill3065()
    {
        driver.FindElement(By.Id("Contato_nome")).SendKeys(""); // Preenche o campo de nome do contato.
        driver.FindElement(By.Id("SetorFiscal_email")).SendKeys(""); // Preenche o campo de e-mail do setor fiscal.
        driver.FindElement(By.Id("Contato_telefone")).SendKeys("51"); // Preenche o telefone do contato.
        driver.FindElement(By.Id("SetorFiscal_telefone")).SendKeys("51"); // Preenche o telefone do setor fiscal.
        driver.FindElement(By.Id("Nota_numero")).SendKeys("3123"); // Preenche o número da nota.

        DateTime currentDate = DateTime.Now; // Obtém a data atual.
        string formattedDate = currentDate.ToString("dd/MM/yyyy"); // Formata a data.
        driver.FindElement(By.Id("Emissao")).SendKeys(formattedDate); // Preenche a data de emissão.
    }

    // Método para atualizar as datas no arquivo XML.
    public void UpdateDates()
    {
        originalXmlFilePath = ""; // Define o caminho do arquivo XML original.
        XmlDocument xmlDoc = new XmlDocument(); // Cria um novo documento XML.
        xmlDoc.Load(originalXmlFilePath); // Carrega o arquivo XML.
        
        XmlNodeList competenciaNodes = xmlDoc.GetElementsByTagName("Competencia"); // Obtém os nós de competência.
        if (competenciaNodes.Count > 0)
        {
            originalCompetenciaValue = competenciaNodes[0].InnerText; // Armazena o valor original da competência.
        }

        XmlNodeList dataEmissaoNodes = xmlDoc.GetElementsByTagName("DataEmissao"); // Obtém os nós de data de emissão.
        if (dataEmissaoNodes.Count > 0)
        {
            originalDataEmissaoValue = dataEmissaoNodes[0].InnerText; // Armazena o valor original da data de emissão.
        }
        
        DateTime currentDate = DateTime.Now; // Obtém a data atual.
        string formattedDate = currentDate.ToString("yyyy-MM-dd"); // Formata a data.
        
        if (competenciaNodes.Count > 0)
        {
            competenciaNodes[0].InnerText = formattedDate; // Atualiza o valor da competência.
        }

        if (dataEmissaoNodes.Count > 0)
        {
            dataEmissaoNodes[0].InnerText = formattedDate; // Atualiza o valor da data de emissão.
        }

        xmlDoc.Save(originalXmlFilePath); // Salva as alterações no arquivo XML.
        driver.FindElement(By.Id("Nota_xml")).SendKeys(originalXmlFilePath); // Preenche o campo XML com o caminho do arquivo.
    }

    // Método para aguardar o preenchimento do campo CNPJ.
    public void WaitForCnpjFieldToBeFilled(IWebDriver driver)
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); // Configura espera de 10 segundos.
        wait.Until(driver => !string.IsNullOrEmpty(driver.FindElement(By.Id("Cnpj")).GetAttribute("value"))); // Espera até que o campo CNPJ esteja preenchido.
    }

    // Método para retornar as datas ao estado original.
    public void ReturnDates()
    {
        if (!string.IsNullOrEmpty(originalXmlFilePath) && File.Exists(originalXmlFilePath)) // Verifica se o caminho do arquivo original não está vazio e se o arquivo existe.
        {
            XmlDocument xmlDoc = new XmlDocument(); // Cria um novo documento XML.
            xmlDoc.Load(originalXmlFilePath); // Carrega o arquivo XML.
            XmlNodeList competenciaNodes = xmlDoc.GetElementsByTagName("Competencia"); // Obtém os nós de competência.
            XmlNodeList dataEmissaoNodes = xmlDoc.GetElementsByTagName("DataEmissao"); // Obtém os nós de data de emissão.
            
            if (competenciaNodes.Count > 0)
            {
                competenciaNodes[0].InnerText = originalCompetenciaValue; // Retorna o valor original da competência.
            }

            if (dataEmissaoNodes.Count > 0)
            {
                dataEmissaoNodes[0].InnerText = originalDataEmissaoValue; // Retorna o valor original da data de emissão.
            }

            xmlDoc.Save(originalXmlFilePath); // Salva as alterações no arquivo XML.
        }
    }
}
