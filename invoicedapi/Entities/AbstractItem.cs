using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;

namespace Invoiced
{
	
	public abstract class AbstractItem {
		protected AbstractItem() {

		}

		public override string ToString(){
				var s = base.ToString() + "<" + this.EntityId() +">";
				var jsonS =  s + " " + this.ToJsonString();

				return jsonS;
		}

		public string ToJsonString() {
				return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore } );
		}

		protected abstract string EntityId();

	}

}
