using System;
using System.Runtime.Serialization;

namespace Plugins.AzureModels
{
	[DataContract]
	public class MSNFP_Registration
	{
		[DataMember]
		public Guid RegistrationId
		{
			get;
			set;
		}

		[DataMember]
		public string FirstName
		{
			get;
			set;
		}

		[DataMember]
		public string LastName
		{
			get;
			set;
		}

		[DataMember]
		public string Email
		{
			get;
			set;
		}

		[DataMember]
		public string Telephone
		{
			get;
			set;
		}

		[DataMember]
		public string Address_Line1
		{
			get;
			set;
		}

		[DataMember]
		public string Address_Line2
		{
			get;
			set;
		}

		[DataMember]
		public string Address_City
		{
			get;
			set;
		}

		[DataMember]
		public string Address_Province
		{
			get;
			set;
		}

		[DataMember]
		public string Address_PostalCode
		{
			get;
			set;
		}

		[DataMember]
		public string Address_Country
		{
			get;
			set;
		}

		[DataMember]
		public Guid? TableId
		{
			get;
			set;
		}

		[DataMember]
		public string Team
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? CreatedOn
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? SyncDate
		{
			get;
			set;
		}

		[DataMember]
		public bool Deleted
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? DeletedDate
		{
			get;
			set;
		}

		[DataMember]
		public Guid? CustomerId
		{
			get;
			set;
		}

		[DataMember]
		public int? CustomerIdType
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? Date
		{
			get;
			set;
		}

		[DataMember]
		public Guid? EventId
		{
			get;
			set;
		}

		[DataMember]
		public Guid? EventPackageId
		{
			get;
			set;
		}

		[DataMember]
		public Guid? TicketId
		{
			get;
			set;
		}

		[DataMember]
		public string GroupNotes
		{
			get;
			set;
		}

		[DataMember]
		public Guid? EventTicketId
		{
			get;
			set;
		}

		[DataMember]
		public string Identifier
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Firstname
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_LastName
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Emailaddress1
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Telephone1
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_City
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_Country
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_Line1
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_Line2
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_Line3
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_Postalcode
		{
			get;
			set;
		}

		[DataMember]
		public string msnfp_Billing_StateorProvince
		{
			get;
			set;
		}

		[DataMember]
		public Guid? TransactionCurrencyId
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
