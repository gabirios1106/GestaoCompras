using GestaoCompras.Web.Interfaces.Common;
using MudBlazor;
using System.Net;

namespace GestaoCompras.Web.Services.Common;


public class HttpMessageService(ISnackbarService snackbarService, ILogger<HttpMessageService> logger) : IHttpMessageService
{
    private readonly ISnackbarService _snackbarService = snackbarService;
    private readonly ILogger<HttpMessageService> _logger = logger;

    private Severity _severityLevel;
    private LogLevel _logLevel;
    private string _messageTitle;
    private string _messageDescription;
    private string _messageToast;
    private string _messageTip;
    private bool _showTip;
    private bool _msgError;

    public void ShowMessage(HttpStatusCode httpStatusCode, string customMessage, int timeout, string baseAddress, string routeAPI = null, string reasonPhrase = null, string responseContent = null)
    {
        //Lista de erros HTTP: https://developer.mozilla.org/pt-BR/docs/Web/HTTP/Status

        _logger.LogInformation("Preparando mensagem para o código HTTP {HttpStatusCode}", httpStatusCode);

        switch (httpStatusCode)
        {
            case HttpStatusCode.OK: //200
                _messageTitle = "Sucesso";
                _messageDescription = customMessage;
                _severityLevel = Severity.Success;
                _logLevel = LogLevel.Information;
                _showTip = false;
                //Nesse caso vai aplicar o timeout enviado na chamada
                _msgError = false;
                break;

            case HttpStatusCode.BadRequest: //400
                _messageTitle = "Erro";
                _messageDescription = $"{responseContent}";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = false;
                timeout = 5000;
                _msgError = true;
                break;

            case HttpStatusCode.Unauthorized: //401
                _messageTitle = "Erro";
                _messageDescription = $"{responseContent}";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = false;
                timeout = 3500;
                _msgError = true;
                break;

            case HttpStatusCode.Forbidden: //403
                _messageTitle = "Erro";
                _messageDescription = $"{responseContent}";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = true;
                timeout = 3500;
                _msgError = true;
                break;

            case HttpStatusCode.NotFound: //404
                _messageTitle = "Erro";
                _messageDescription = $"{responseContent}";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = false;
                timeout = 3500;
                _msgError = true;
                break;

            case HttpStatusCode.MethodNotAllowed: //405
                _messageTitle = "Erro";
                _messageDescription = $"Método não permitido - {responseContent}";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = false;
                timeout = 3500;
                _msgError = true;
                break;

            case HttpStatusCode.InternalServerError: //500
                _messageTitle = "Erro";
                _messageDescription = $"Falha interna do servidor";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = true;
                timeout = 10000;
                _msgError = true;
                break;

            default: //Outros
                _messageTitle = "Erro";
                _messageDescription = $"Desconhecido - {responseContent}";
                _severityLevel = Severity.Error;
                _logLevel = LogLevel.Error;
                _showTip = true;
                timeout = 10000;
                _msgError = true;
                break;
        }

        if (_msgError)
        {
            routeAPI = baseAddress + routeAPI;
            _messageToast = $"{_messageDescription}.";

            _messageTip = (_showTip) ? "(Pressione F12 para verificar mais detalhes no console do navegador)" : null;

            _snackbarService.ShowSnackbar(_messageTitle, _messageToast, _severityLevel, timeout, _messageTip);

            _logger.Log(_logLevel, "Erro ao acessar {RouteAPI} - [{HttpStatusCode}] {ResponseContent}", routeAPI, httpStatusCode, responseContent);
        }
        else
            _snackbarService.ShowSnackbar(_messageTitle, _messageDescription, _severityLevel, timeout);
    }
}
