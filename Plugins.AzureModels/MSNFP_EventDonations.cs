using System;
using System.Runtime.Serialization;

namespace Plugins.AzureModels
{
	[DataContract]
	public class MSNFP_EventDonations
	{
		[DataMember]
		public Guid EventDonationId
		{
			get;
			set;
		}

		[DataMember]
		public decimal? Amount
		{
			get;
			set;
		}

		[DataMember]
		public string Description
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
		public string Name
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
