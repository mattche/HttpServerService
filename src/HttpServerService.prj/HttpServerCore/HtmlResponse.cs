using System;
using System.Net;

namespace HttpServerCore
{
	/// <summary>Статический класс, предоставляющий данные и методы 
	/// для формирования HTML-документов.</summary>
	public static class HtmlResponse
	{
		#region Methods

		/// <summary>Генерирует код HTML-документа, соответсвующего коду статуса HTTP-ответа.</summary>
		/// <param name="statusCode">Код статуса HTTP</param>
		/// <returns>Строка, представляющая HTML-код</returns>
		public static string GetHtmlResponse(HttpStatusCode statusCode)
		{
			switch (statusCode)
			{
				case HttpStatusCode.BadRequest:
					return string.Format(_errorResponse, (int)statusCode, 
						"Некорректный запрос");
				case HttpStatusCode.InternalServerError:
					return string.Format(_errorResponse, (int)statusCode,
						"Ошибка на сервере");
				default:
					return _helloResponse;
			}
		}

		#endregion

		#region Data

		/// <summary>Код HTML-документа, содержащего приветствие.</summary>
		private static readonly string _helloResponse = 
			"<html>" +
			"<head></head>" +
			"<body>" +
			"<p>Привет!</p>" +
			"Добро пожаловать!" +
			"</body>" +
			"</html>";

		/// <summary>Код HTML-документа, содержащего шаблон сообщения об ошибке.</summary>
		private static readonly string _errorResponse =
			"<html>" +
			"<head></head>" +
			"<body>" +
			"<p><b>Error {0}</b></p>" +
			"{1}" +
			"</body>" +
			"</html>";

		#endregion
	}
}
