using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Invoiced
{

	public class Coupon : AbstractEntity<Coupon> {

		internal Coupon() : base() {
			this.EntityName = "/coupons";
		}

		internal Coupon(Connection conn) : base(conn) {
			this.EntityName = "/coupons";
		}

		protected override string EntityId() {
			return this.Id;
		}

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("object")]
		public string Obj { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("value")]
		public double? Value { get; set; }

        [JsonProperty("is_percent")]
		public bool? IsPercent { get; set; }

        [JsonProperty("exclusive")]
		public bool? Exclusive { get; set; }

        [JsonProperty("expiration_date")]
		public long? ExpirationDate { get; set; }

        [JsonProperty("max_redemptions")]
		public long? MaxRedemptions { get; set; }

		[JsonProperty("created_at")]
		public long? CreatedAt { get; set; }

		[JsonProperty("metadata")]
		public Metadata Metadata { get; set; }

		// Conditional Serialisation

		public bool ShouldSerializeId() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeObj() {
			return false;
		}

		public bool ShouldSerializeCurrency() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeValue() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeIsPercent() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeExclusive() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeExpirationDate() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeMaxRedemptions() {
			return this.CurrentOperation == "Create";
		}

		public bool ShouldSerializeCreatedAt() {
			return false;
		}

	}

}
