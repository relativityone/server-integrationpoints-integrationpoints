﻿using System.Collections.Generic;
using kCura.Crypto.DataProtection;

namespace kCura.IntegrationPoints.Core.Services.Marshaller
{
	internal class SecureAppDomainDataMarshaller : IAppDomainDataMarshaller
	{
		private readonly DataProtector _dataProtector;

		internal SecureAppDomainDataMarshaller()
		{
			_dataProtector = new DataProtector(Store.MachineStore);
		}

		public void MarshalDataToDomain(System.AppDomain targetAppDomain, IDictionary<string, byte[]> dataDictionary)
		{
			foreach (KeyValuePair<string, byte[]> entry in dataDictionary)
			{
				byte[] value = entry.Value;
				byte[] encryptedData = null;
				if (value != null)
				{
					encryptedData = _dataProtector.Encrypt(value);
				}

				targetAppDomain.SetData(entry.Key, encryptedData);
			}
		}

		public byte[] RetrieveMarshaledData(System.AppDomain sourceAppDomain, string dataKey)
		{
			byte[] encryptedData = sourceAppDomain.GetData(dataKey) as byte[];
			if (encryptedData == null || encryptedData.Length == 0)
			{
				return null;
			}

			byte[] decryptedData = _dataProtector.Decrypt(encryptedData);

			return decryptedData;
		}
	}
}