using System;
using System.Net;

namespace HttpServerCore
{
	/// <summary>Предоставляет методы проверки данных соединения на корректность, а также 
	/// их значения по умолчанию.</summary>
	public static class Validation
	{
		/// <summary>Указывает, является ли входная строка, представляющая собой адрес хоста, 
		/// корректно записанным универсальным кодом ресурса (URI).</summary>
		/// <param name="host">Строка, представляющая URI хоста.</param>
		/// <returns>Значение true, если входная строка является корректным URI, значение false,
		/// если нет.</returns>
		public static bool ValidateHost(string host)
		{
			return	!string.IsNullOrEmpty(host) 
					&& Uri.IsWellFormedUriString(host, UriKind.Absolute);
		}

		/// <summary>
		/// Указывает, является ли число, представляющее номер порта,
		/// корректным значением порта.</summary>
		/// <param name="port">Число, представляющее номер порта</param>
		/// <returns>Значение true, если входная величина является корректным номером порта,
		/// значение false, если нет.</returns>
		public static bool ValidatePort(int port)
		{
			return	port >= IPEndPoint.MinPort 
					&& port <= IPEndPoint.MaxPort;
		}

		/// <summary>Возвращает значение URI локального узла по умолчанию.</summary>
		/// <value>Строка, представляющая URI хоста.</value>
		public static string DefaultHost
		{
			get 
			{ 
				return "http://127.0.0.1"; 
			}
		} 

		/// <summary>Возвращает значение номера порта по умолчанию.</summary>
		/// <value>Число, представляющее номер порта.</value>
		public static int DefaultPort
		{
			get 
			{ 
				return 80; 
			}
		}
	}
}
