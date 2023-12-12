using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using SendGrid;
using SendGrid.Helpers.Mail;
using ByteWave.Models;

namespace ByteWave.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public HomeController(ILogger<HomeController> logger, ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _logger = logger;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        [HttpPost]
            public async Task<IActionResult> SendEmail([FromBody] ContactFormModel model)
            {
                try
                {
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);

                    var from = new EmailAddress("zcgamerbr@gmail.com", "Byte Wave");
                    var to = new EmailAddress(model.Email, "Recipient");

                    var subject = model.Subject;
                    var plainTextContent = $"Nome: {model.Name}\nE-mail: {model.Email}\nMensagem:\n{model.Message}";
                    var htmlContent = $"<strong>Nome:</strong> {model.Name}<br/><strong>E-mail:</strong> {model.Email}<br/><strong>Mensagem:</strong><br/>{model.Message}<br/>Byte Wave Inovação e excelência em desenvolvimento de software para transformar sua visão em realidade. <br/> Logo mais Entraremos em Contado";

                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                    var response = await client.SendEmailAsync(msg);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        _logger.LogError($"Erro ao enviar e-mail via SendGrid. StatusCode: {response.StatusCode}");
                        return Json(new { success = false, message = "Erro ao enviar e-mail. Tente novamente mais tarde." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao enviar e-mail via SendGrid: {ex.Message}");
                    return Json(new { success = false, message = "Erro ao enviar e-mail. Tente novamente mais tarde." });
                }
            }
       

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {   
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.Success)
            {
                using (var writer = new StringWriter())
                {
                    var viewContext = new ViewContext(
                        ControllerContext,
                        viewResult.View,
                        new ViewDataDictionary(
                            new EmptyModelMetadataProvider(), 
                            new ModelStateDictionary()
                        )
                        {
                            Model = model
                        },
                        new TempDataDictionary(ControllerContext.HttpContext, _tempDataProvider),
                        writer,
                        new HtmlHelperOptions()
                    );

                    await viewResult.View.RenderAsync(viewContext);
                    return writer.ToString();
                }
            }

            throw new InvalidOperationException($"A view '{viewName}' não foi encontrada.");
        }
    }
}

