using System;
using System.Runtime.Serialization;

namespace Plugins.AzureModels
{
	[DataContract]
	public class MSNFP_Designation
	{
		[DataMember]
		public Guid DesignationId
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
	}
}
