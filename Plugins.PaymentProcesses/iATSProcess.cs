using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Plugins.PaymentProcesses
{
	public class iATSProcess
	{
		public static XmlDocument CreateCreditCardCustomerCode(CreateCreditCardCustomerCode obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/CustomerLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/CreateCreditCardCustomerCode");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(CreateCreditCardCustomerCode), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument GetCustomerCodeDetail(GetCustomerCodeDetail obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/CustomerLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/GetCustomerCodeDetail");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetCustomerCodeDetail), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument ProcessCreditCardWithCustomerCode(ProcessCreditCardWithCustomerCode obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/ProcessLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/ProcessCreditCardWithCustomerCode");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProcessCreditCardWithCustomerCode), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument CreateACHEFTCustomerCode(CreateACHEFTCustomerCode obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/CustomerLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/CreateACHEFTCustomerCode");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(CreateACHEFTCustomerCode), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument ProcessACHEFTWithCustomerCode(ProcessACHEFTWithCustomerCode obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/ProcessLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/ProcessACHEFTWithCustomerCode");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProcessACHEFTWithCustomerCode), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument UpdateCreditCardCustomerCode(UpdateCreditCardCustomerCode obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/CustomerLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/UpdateCreditCardCustomerCode");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateCreditCardCustomerCode), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument UpdateACHEFTCustomerCode(UpdateACHEFTCustomerCode obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/CustomerLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/UpdateACHEFTCustomerCode");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateACHEFTCustomerCode), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument ProcessCreditCardRefundWithTransactionId(ProcessCreditCardRefundWithTransactionId obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/ProcessLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/ProcessCreditCardRefundWithTransactionId");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProcessCreditCardRefundWithTransactionId), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static XmlDocument ProcessACHEFTRefundWithTransactionId(ProcessACHEFTRefundWithTransactionId obj)
		{
			XmlDocument xmlDocument = null;
			try
			{
				string requestUriString = "https://www.iatspayments.com/netgate/ProcessLinkv2.asmx";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
				webHeaderCollection.Add("SOAPAction", "https://www.iatspayments.com/NetGate/ProcessACHEFTRefundWithTransactionId");
				httpWebRequest.Headers = webHeaderCollection;
				httpWebRequest.KeepAlive = false;
				httpWebRequest.ContentType = "text/xml";
				httpWebRequest.MediaType = "application/xml";
				httpWebRequest.Accept = "application/xml";
				httpWebRequest.Method = "POST";
				string arg = string.Empty;
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProcessACHEFTRefundWithTransactionId), "https://www.iatspayments.com/NetGate/");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
					XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
					xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
					arg = Encoding.UTF8.GetString(memoryStream.ToArray());
				}
				XmlDocument xmlDocument2 = new XmlDocument();
				xmlDocument2.LoadXml($"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n                <soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n                  <soap:Body>\r\n                    {arg}\r\n                  </soap:Body>\r\n                </soap:Envelope>");
				using (Stream outStream = httpWebRequest.GetRequestStream())
				{
					xmlDocument2.Save(outStream);
				}
				WebResponse response = httpWebRequest.GetResponse();
				XmlDocument xmlDocument3 = new XmlDocument();
				XmlReader reader = XmlReader.Create(response.GetResponseStream());
				xmlDocument3.Load(reader);
				return xmlDocument3;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
