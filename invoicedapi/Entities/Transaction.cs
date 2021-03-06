
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Invoiced
{

	public class Transaction : AbstractEntity<Transaction>
	{
	
		internal Transaction(Connection conn) : base(conn) {
			this.EntityName = "/transactions";
		}

		public Transaction() : base(){
			this.EntityName = "/transactions";
		}

		protected override string EntityId() {
			return this.Id.ToString();
		}

		[JsonProperty("id")]
		public long? Id { get; set; }

		[JsonProperty("object")]
		public string Obj { get; set; }

		[JsonProperty("customer")]
		public long? Customer { get; set; }

		[JsonProperty("invoice")]
		public long? Invoice { get; set; }

		[JsonProperty("credit_note")]
		public long? CreditNote { get; set; }

		[JsonProperty("date")]
		public long? Date { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("method")]
		public string Method { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("gateway")]
		public string Gateway { get; set; }

		[JsonProperty("gateway_id")]
		public string GatewayId { get; set; }

		[JsonProperty("payment_source")]
		public PaymentSource PaymentSource { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("amount")]
		public double? Amount { get; set; }

		[JsonProperty("notes")]
		public string Notes { get; set; }

		[JsonProperty("failure_reason")]
		public string FailureReason { get; set; }

		[JsonProperty("parent_transaction")]
		public long? ParentTransaction { get; set; }

		[JsonProperty("pdf_url")]
		public string PdfUrl { get; set; }

		[JsonProperty("created_at")]
		public long? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public long? UpdatedAt { get; set; }


		[JsonProperty("metadata")]
		public Metadata Metadata { get; set; }

		public Transaction InitiateCharge(ChargeRequest chargeRequest) {

			string url = "/charges";
			string jsonRequestBody = chargeRequest.ToJsonString();

			string responseText = this.GetConnection().Post(url,null,jsonRequestBody);
			Transaction serializedObject;
			
			try {
					serializedObject = JsonConvert.DeserializeObject<Transaction>(responseText,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
			} catch(Exception e) {
				throw new EntityException("",e);
			}

			return serializedObject;

		}

		public Transaction Refund(long amount) {

			string url = this.GetEndpoint(true) + "/refunds";
			string jsonRequestBody = "{'amount': " + amount.ToString() + "}";

			string responseText = this.GetConnection().Post(url,null,jsonRequestBody);
			Transaction serializedObject;
			
			try {
				serializedObject = JsonConvert.DeserializeObject<Transaction>(responseText,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
			} catch(Exception e) {
				throw new EntityException("",e);
			}

			return serializedObject;

		}

		// Conditional Serialisation

		public bool ShouldSerializeId() {
			return false;
		}

		public bool ShouldSerializeObj() {
			return false;
		}

		public bool ShouldSerializeCustomer() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeInvoice() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeCreditNote() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeType() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeFailureReason() {
			return false;
		}

		public bool ShouldSerializeParentTransaction() {
			return false;
		}

		public bool ShouldSerializePdfUrl() {
			return false;
		}

		public bool ShouldSerializeCreatedAt() {
			return false;
		}

		public bool ShouldSerializeUpdatedAt() {
			return false;
		}
	
	}

}
