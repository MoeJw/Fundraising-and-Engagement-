using System;
using System.Runtime.Serialization;

namespace Plugins.AzureModels
{
	[DataContract]
	public class MSNFP_RegistrationPreference
	{
		[DataMember]
		public Guid registrationpreferenceid
		{
			get;
			set;
		}

		[DataMember]
		public Guid? registrationid
		{
			get;
			set;
		}

		[DataMember]
		public Guid? eventpreference
		{
			get;
			set;
		}

		[DataMember]
		public Guid? eventid
		{
			get;
			set;
		}

		[DataMember]
		public string other
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? createdon
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? syncdate
		{
			get;
			set;
		}

		[DataMember]
		public bool deleted
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? deleteddate
		{
			get;
			set;
		}

		[DataMember]
		public int? StateCode
		{
			get;
			set;
		}

		[DataMember]
		public int? StatusCode
		{
			get;
			set;
		}
	}
}
