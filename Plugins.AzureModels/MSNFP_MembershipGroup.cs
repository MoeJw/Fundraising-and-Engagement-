using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Plugins.AzureModels
{
	[DataContract]
	public class MSNFP_MembershipGroup
	{
		[DataMember]
		public Guid MembershipGroupId
		{
			get;
			set;
		}

		[DataMember]
		public string GroupName
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
		public DateTime? SyncDate
		{
			get;
			set;
		}

		[DataMember]
		public bool? Deleted
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
		public DateTime? CreatedOn
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

		[DataMember]
		public virtual ICollection<MSNFP_MembershipOrder> MembershipOrder
		{
			get;
			set;
		}
	}
}
