using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;

namespace HttpServerCore
{
	/// <summary>предоставляет функциональность сервера HTTP, который
	/// ожидает приходящие на локальный узел HTTP-запросы и отвечает на них.</summary>
	public class HttpServer
	{
		#region .ctor

		/// <summary>Создаёт <see cref="HttpServer"/>.</summary>
		public HttpServer() :
			this(Validation.DefaultHost, Validation.DefaultPort) {}

		/// <summary>Создаёт <see cref="HttpServer"/>, который будет обрабатывать запросы, поступающие 
		/// на локальный узел с заданным URI и портом.</summary>
		/// <param name="host">Строка, хранящая URI, описывающий адрес хоста с использованием 
		/// доменного имени или IP-адреса хоста.</param>
		/// <param name="port">Число, представляющее номер порта хоста.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="PlatformNotSupportedException"></exception>
		public HttpServer(string host, int port)
		{
			if (Validation.ValidateHost(host) && Validation.ValidatePort(port))
				HostUri = string.Format("{0}:{1}/", host, port);
			else
				throw new ArgumentException(string.Format("Невозможно применить заданные " +
					"значения параметров {0} и {1}.", nameof(host), nameof(port)));

			_httpListener = new HttpListener();
			_httpListener.Prefixes.Add(HostUri);
			
		}

		#endregion

		#region Methods

		/// <summary>Инициирует начало цикла обработки запросов.</summary>
		/// <exception cref="HttpListenerException"></exception>
		public void Start()
		{
			ServerLogEvent(this, "Начало работы");
			ServerLogEvent(this, string.Format("Прослушивается {0}", HostUri));
			
			_httpListener.Start();
			
			var image = AppResources.img;
			var regexImgJpeg = new Regex(_templateImgJpeg);
			var regexImgPng = new Regex(_templateImgPng);
			var regexText = new Regex(_templateText);

			while (true)
			{
				ServerLogEvent(this, "Ожидание входящих соединений...");

				var context = _httpListener.GetContext();
				var request = context.Request;
				var response = context.Response;

				ServerLogEvent(this, string.Format("{0} запрос на Url {1} был получен от {2}",
					request.HttpMethod, request.Url, request.RemoteEndPoint));

				var clientEncoding = request.ContentEncoding;
				var requestParam = request.RawUrl;

				try
				{
					if (regexImgJpeg.IsMatch(requestParam, 0))
					{
						SendImage(response, image, ImageFormat.Jpeg);
					}
					if (regexImgPng.IsMatch(requestParam, 0))
					{
						SendImage(response, image, ImageFormat.Png);
					}
					else if (regexText.IsMatch(requestParam, 0))
					{
						var statusCode = HttpStatusCode.OK;
						SendHtml(response, statusCode, HtmlResponse.GetHtmlResponse(statusCode),
							clientEncoding);
					}
					else
					{
						var statusCode = HttpStatusCode.BadRequest;
						SendHtml(response, statusCode, HtmlResponse.GetHtmlResponse(statusCode),
							clientEncoding);
					}
				}
				catch
				{
					var statusCode = HttpStatusCode.InternalServerError;
					response.StatusCode = (int)statusCode;
					SendHtml(response, statusCode, HtmlResponse.GetHtmlResponse(statusCode),
						clientEncoding);
				}
			}
		}

		/// <summary>
		/// Отправляет HTTP-ответ с заданным статусом и телом ответа 
		/// в виде HTML-документа указанной кодировки.</summary>
		/// <param name="response">HTTP-ответ на полученный ранее запрос</param>
		/// <param name="statusCode">Статус ответа</param>
		/// <param name="htmlResponse">Отправляемый код HTML-документа</param>
		/// <param name="clientEncoding">Кодировка клиента, выполнившего запрос</param>
		private void SendHtml(HttpListenerResponse response, HttpStatusCode statusCode,
			string htmlResponse, Encoding clientEncoding)
		{
			response.StatusCode = (int)statusCode;
			response.ContentType = "text/html";
			using (response.OutputStream)
			{
				var resopneBytes = clientEncoding.GetBytes(htmlResponse);
				response.OutputStream.Write(resopneBytes, 0, resopneBytes.Length);
			}
			response.Close();
			ServerLogEvent(this, "Ответ в виде HTML-документа отправлен клиенту");
		}

		/// <summary>Отправляет HTTP-ответ с телом ответа 
		/// в виде изображения указанного формата.</summary>
		/// <param name="response">HTTP-ответ на полученный ранее запрос</param>
		/// <param name="image">Изображение</param>
		/// <param name="format">Формат изображения</param>
		private void SendImage(HttpListenerResponse response, Bitmap image, ImageFormat format)
		{
			var statusCode = HttpStatusCode.OK;
			response.StatusCode = (int)statusCode;
			response.ContentType = string.Format("image/{0}", format.ToString().ToLower());
			using(response.OutputStream)
			{
				image.Save(response.OutputStream, ImageFormat.Jpeg);
			}
			response.Close();
			ServerLogEvent(this, string.Format("Ответ в виде  {0}-изображения отправлен клиенту", 
				format.ToString()));
		}

		#endregion

		#region Events

		/// <summary>Событие лога HTTP-сервера.</summary>
		public event EventHandler<string> ServerLogEvent;

		#endregion

		#region Properties

		/// <summary>Возвращает URI локального узла.</summary>
		/// <value>Строка, представляющая URI узла.</value>
		public string HostUri { get; private set; }

		#endregion

		#region Data

		/// <summary>Предоставляет функциональность контролируемого прослушивателя HTTP.</summary>
		private HttpListener _httpListener;
		/// <summary>Шаблон запроса на изображение.</summary>
		private readonly string _templateImgJpeg = "/img/jpeg/?";
		/// <summary>Шаблон запроса на изображение.</summary>
		private readonly string _templateImgPng = "/img/png/?";
		/// <summary>Шаблон запроса на текст.</summary>
		private readonly string _templateText = "/text/?";

		#endregion
	}
}
