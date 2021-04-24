using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeLegalEntityVerification : StripeEntity
	{
		[JsonProperty("details")]
		public string Details
		{
			get;
			set;
		}

		[JsonProperty("details_code")]
		public string DetailsCode
		{
			get;
			set;
		}

		public string DocumentId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeFileUpload Document
		{
			get;
			set;
		}

		[JsonProperty("document")]
		internal object InternalDocument
		{
			set
			{
				ExpandableProperty<StripeFileUpload>.Map(value, delegate(string s)
				{
					DocumentId = s;
				}, delegate(StripeFileUpload o)
				{
					Document = o;
				});
			}
		}

		[JsonProperty("status")]
		public string Status
		{
			get;
			set;
		}
	}
}
